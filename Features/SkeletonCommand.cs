using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Num +: forces every friendly skeleton following you (within 50 m) to
    /// attack the creature under your crosshair at the moment of the command.
    /// Aimed at a piece/structure instead (T.W.I.G. training dummy, doors,
    /// ...)? Those are attacked through the MonsterAI static-target path.
    /// Pressed with nothing in the crosshair, it recalls the skeletons.
    /// The AI target fields are written directly (MonsterAI.SetTarget refuses
    /// to run on skeletons that already have a target) and the command is
    /// re-asserted a few times per second: the vanilla AI drops targets when
    /// its "time since sensed" decay timer is over 30 s, which is always the
    /// case for skeletons idling at a peaceful base. When the target dies or
    /// is destroyed, re-asserting stops and skeletons fall back to their
    /// default follow behaviour. Players and skeleton minions are not valid
    /// targets.
    /// </summary>
    internal static class SkeletonCommand
    {
        private const string Prefix = "SkeletonCommand";
        private const string FriendlySkeletonPrefix = "Skeleton_Friendly";
        private const float CommandRadius = 50f;
        private const float ReassertInterval = 0.25f;

        private static readonly FieldInfo HoveringCreatureField =
            AccessTools.Field(typeof(Player), "m_hoveringCreature");

        private static readonly FieldInfo HoveringField =
            AccessTools.Field(typeof(Player), "m_hovering");

        private static readonly FieldInfo TargetCreatureField =
            AccessTools.Field(typeof(MonsterAI), "m_targetCreature");

        private static readonly FieldInfo LastKnownTargetPosField =
            AccessTools.Field(typeof(MonsterAI), "m_lastKnownTargetPos");

        private static readonly FieldInfo BeenAtLastPosField =
            AccessTools.Field(typeof(MonsterAI), "m_beenAtLastPos");

        private static readonly FieldInfo TargetStaticField =
            AccessTools.Field(typeof(MonsterAI), "m_targetStatic");

        private static readonly FieldInfo TimeSinceSensedField =
            AccessTools.Field(typeof(MonsterAI), "m_timeSinceSensedTargetCreature");

        private static readonly FieldInfo TimeSinceAttackingField =
            AccessTools.Field(typeof(MonsterAI), "m_timeSinceAttacking");

        private static readonly MethodInfo SetAlertedMethod =
            AccessTools.Method(typeof(BaseAI), "SetAlerted");

        // Active squad command: re-asserted until the target is gone.

        private static readonly List<Character> CommandedSkeletons = new();
        private static Character _commandTarget;
        private static StaticTarget _commandStaticTarget;
        private static float _nextReassert;

        private static float _nextProbe;

        private static readonly FieldInfo CanBeAlertedField =
            AccessTools.Field(typeof(BaseAI), "m_canBeAlerted");

        public static void CommandSkeletons()
        {
            try
            {
                Player player = GlobalState.Player ?? Player.m_localPlayer;
                if (player == null)
                {
                    NotificationManager.Notification("No player - can't command summons.", MessageHud.MessageType.TopLeft);
                    return;
                }

                Character target = HoveringCreatureField?.GetValue(player) as Character;
                if (target != null)
                {
                    // A dead or invalid aimed character degrades to a recall.
                    if (target.IsDead())
                        target = null;
                    else if (target.IsPlayer())
                    {
                        NotificationManager.Notification("Can't command summons against players.", MessageHud.MessageType.TopLeft);
                        return;
                    }
                    else if (target.name.StartsWith(FriendlySkeletonPrefix, StringComparison.Ordinal))
                    {
                        NotificationManager.Notification("Can't command summons against other summons.", MessageHud.MessageType.TopLeft);
                        return;
                    }
                }

                // Aimed at a piece/structure instead of a creature (T.W.I.G.
                // training dummy, doors, ...)? Those are attacked through the
                // MonsterAI static-target path.
                StaticTarget staticTarget = null;
                if (target == null && HoveringField?.GetValue(player) is GameObject hovered && hovered != null)
                    staticTarget = hovered.GetComponentInParent<StaticTarget>();

                bool attack = target != null || staticTarget != null;
                string targetName = attack ? (target != null ? target.GetHoverName() : StaticTargetName(staticTarget)) : null;
                long playerId = player.GetPlayerID();
                int commanded = 0;

                CommandedSkeletons.Clear();
                _commandTarget = attack ? target : null;
                _commandStaticTarget = attack ? staticTarget : null;

                foreach (Character skeleton in Character.GetAllCharacters().ToArray())
                {
                    if (skeleton == null || skeleton.IsDead())
                        continue;

                    if (!skeleton.name.StartsWith(FriendlySkeletonPrefix, StringComparison.Ordinal))
                        continue;

                    if (Vector3.Distance(skeleton.transform.position, player.transform.position) > CommandRadius)
                        continue;

                    MonsterAI ai = skeleton.GetComponent<MonsterAI>();
                    GameObject followTarget = ai != null ? ai.GetFollowTarget() : null;
                    Player owner = followTarget != null ? followTarget.GetComponent<Player>() : null;
                    if (owner == null || owner.GetPlayerID() != playerId)
                        continue;

                    if (attack)



                    {



                        ApplyCommand(ai, target, staticTarget);



                        CommandedSkeletons.Add(skeleton);



                    }



                    else



                        Recall(ai);
                    commanded++;
                }

                NotificationManager.Notification(
                    commanded == 0
                        ? "No summoned skeletons nearby."
                        : attack
                            ? $"{commanded} skeleton(s) attacking {targetName}."
                            : $"{commanded} skeleton(s) recalled.",
                    MessageHud.MessageType.TopLeft);
                HarmonyLog.Log($"[{Prefix}] {(attack ? "Attack" : "Recall")}: {commanded} skeleton(s)" + (attack ? $", target {targetName}." : "."));
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] CommandSkeletons exception: {ex}.");
            }
        }

        /// <summary>
        /// Per-frame: re-asserts the active command a few times per second,
        /// because the vanilla AI drops targets when its own "time since
        /// sensed" decay timer (30 s) fires - and that timer is already
        /// maxed for skeletons idling at a peaceful base. Stops as soon as
        /// the target dies or is destroyed (default behaviour resumes).
        /// </summary>
        public static void UpdateCommand()
        {
            try
            {
                if (CommandedSkeletons.Count == 0)
                    return;

                bool creatureAlive = _commandTarget != null && !_commandTarget.IsDead();
                bool staticAlive = _commandStaticTarget != null;
                if (!creatureAlive && !staticAlive)
                {
                    // Target gone - AI clears its own target; default follows.
                    CommandedSkeletons.Clear();
                    _commandTarget = null;
                    _commandStaticTarget = null;
                    return;
                }

                if (Time.time < _nextReassert)
                    return;
                _nextReassert = Time.time + ReassertInterval;

                for (int i = CommandedSkeletons.Count - 1; i >= 0; i--)

                {

                    Character skeleton = CommandedSkeletons[i];

                    if (skeleton == null || skeleton.IsDead())

                    {

                        CommandedSkeletons.RemoveAt(i);
                        continue;
                    }

                    MonsterAI ai = skeleton.GetComponent<MonsterAI>();
                    if (ai == null)
                    {
                        CommandedSkeletons.RemoveAt(i);

                        continue;

                    }



                    if (creatureAlive)

                        ForceTarget(ai, _commandTarget);

                    else

                        ForceStaticTarget(ai, _commandStaticTarget);

                }
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] UpdateCommand exception: {ex}.");
            }
        }

        private static string StaticTargetName(StaticTarget target)

        {

            Piece piece = target != null ? target.GetComponent<Piece>() : null;

            if (piece == null)

                return "target";

            return Localization.instance != null ? Localization.instance.Localize(piece.m_name) : piece.m_name;

        }

        /// <summary>
        /// Applies the current command (attack creature / attack static /
        /// recall) to one skeleton.
        /// </summary>
        private static void ApplyCommand(MonsterAI ai, Character target, StaticTarget staticTarget)
        {
            if (target != null)
                ForceTarget(ai, target);
            else if (staticTarget != null)
                ForceStaticTarget(ai, staticTarget);
            else
                Recall(ai);
        }

        /// <summary>
        /// Writes the AI target fields directly (what MonsterAI.SetTarget
        /// does internally, minus its "no current target" guard), alerts the
        /// skeleton so ranged minions engage instead of pathing in, and
        /// resets the AI decay timers that would otherwise discard the
        /// command.
        /// </summary>
        private static void ForceTarget(MonsterAI ai, Character target)
        {
            TargetCreatureField?.SetValue(ai, target);
            TargetStaticField?.SetValue(ai, null);
            LastKnownTargetPosField?.SetValue(ai, target.transform.position);
            BeenAtLastPosField?.SetValue(ai, false);
            TimeSinceSensedField?.SetValue(ai, 0f);
            TimeSinceAttackingField?.SetValue(ai, 0f);
            SetAlertedMethod?.Invoke(ai, new object[] { true });
        }

        private static void ForceStaticTarget(MonsterAI ai, StaticTarget target)
        {
            TargetCreatureField?.SetValue(ai, null);
            TargetStaticField?.SetValue(ai, target);
            LastKnownTargetPosField?.SetValue(ai, target.GetCenter());
            BeenAtLastPosField?.SetValue(ai, false);
            TimeSinceSensedField?.SetValue(ai, 0f);
            TimeSinceAttackingField?.SetValue(ai, 0f);
            SetAlertedMethod?.Invoke(ai, new object[] { true });
        }

        /// <summary>
        /// Clears the forced target and calms the skeleton, so it stops
        /// fighting and returns to its default follow behaviour.
        /// </summary>
        private static void Recall(MonsterAI ai)
        {
            TargetCreatureField?.SetValue(ai, null);
            TargetStaticField?.SetValue(ai, null);
            SetAlertedMethod?.Invoke(ai, new object[] { false });
        }

        /// <summary>
        /// Debug-mode probe (Num * toggles it): once per second, dump the AI
        /// state of owned summoned skeletons so a stuck attack command can
        /// be diagnosed from the log.
        /// </summary>
        public static void DebugProbe()
        {
            try
            {
                if (!GlobalState.Config.Debug || Time.time < _nextProbe)
                    return;

                Player player = GlobalState.Player ?? Player.m_localPlayer;
                if (player == null)
                    return;

                _nextProbe = Time.time + 1f;
                long playerId = player.GetPlayerID();
                foreach (Character skeleton in Character.GetAllCharacters().ToArray())
                {
                    if (skeleton == null || skeleton.IsDead() || !skeleton.name.StartsWith(FriendlySkeletonPrefix, StringComparison.Ordinal))
                        continue;

                    if (Vector3.Distance(skeleton.transform.position, player.transform.position) > CommandRadius)
                        continue;

                    MonsterAI ai = skeleton.GetComponent<MonsterAI>();
                    if (ai == null)
                        continue;

                    object creature = TargetCreatureField?.GetValue(ai);
                    object staticTarget = TargetStaticField?.GetValue(ai);
                    bool alerted = ai.IsAlerted();
                    bool canBeAlerted = CanBeAlertedField != null && CanBeAlertedField.GetValue(ai) is bool value && value;
                    ItemDrop.ItemData weapon = (skeleton as Humanoid)?.GetCurrentWeapon();
                    string weaponInfo = weapon == null
                        ? "weapon=none"
                        : $"weapon={weapon.m_shared.m_name} tt={weapon.m_shared.m_aiTargetType} range={weapon.m_shared.m_aiAttackRange:0.#} min={weapon.m_shared.m_aiAttackRangeMin:0.#} maxAngle={weapon.m_shared.m_aiAttackMaxAngle:0.#}";

                    HarmonyLog.Log($"[{Prefix}] {skeleton.name}: creature={(creature != null ? "yes" : "no")} static={(staticTarget != null ? "yes" : "no")} alerted={alerted} canBeAlerted={canBeAlerted} {weaponInfo}");
                }
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] DebugProbe exception: {ex}.");
            }
        }
    }
}
