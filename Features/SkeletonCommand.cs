using HarmonyLib;

using System;

using System.Linq;

using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Num /: forces every friendly skeleton following you (within 50 m) to
    /// attack the creature under your crosshair at the moment of the command.
    /// Uses the game's own MonsterAI.SetTarget + SetAlerted - the same path a
    /// mob takes when retaliating after being damaged. The AI clears the
    /// target itself when it dies (or stops being a valid enemy), after which
    /// skeletons fall back to their default follow behaviour. Players and
    /// skeleton minions are not valid targets.
    /// </summary>
    internal static class SkeletonCommand
    {
        private const string Prefix = "SkeletonCommand";
        private const string FriendlySkeletonPrefix = "Skeleton_Friendly";
        private const float CommandRadius = 50f;

        private static readonly FieldInfo HoveringCreatureField =
            AccessTools.Field(typeof(Player), "m_hoveringCreature");

        private static readonly MethodInfo SetTargetMethod =
            AccessTools.Method(typeof(MonsterAI), "SetTarget");

        private static readonly MethodInfo SetAlertedMethod =
            AccessTools.Method(typeof(BaseAI), "SetAlerted");

        public static void CommandAttackAimedTarget()
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
                if (target == null || target.IsDead())
                {
                    NotificationManager.Notification("No creature in crosshair - aim at a target first.", MessageHud.MessageType.TopLeft);
                    return;
                }

                if (target.IsPlayer())
                {
                    NotificationManager.Notification("Can't command summons against players.", MessageHud.MessageType.TopLeft);
                    return;
                }

                if (target.name.StartsWith(FriendlySkeletonPrefix, StringComparison.Ordinal))
                {
                    NotificationManager.Notification("Can't command summons against other summons.", MessageHud.MessageType.TopLeft);
                    return;
                }

                long playerId = player.GetPlayerID();
                int commanded = 0;
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

                    SetTargetMethod?.Invoke(ai, new object[] { target });

                    SetAlertedMethod?.Invoke(ai, new object[] { true });

                    commanded++;
                }

                NotificationManager.Notification(
                    commanded > 0
                        ? $"{commanded} skeleton(s) attacking {target.GetHoverName()}."
                        : "No summoned skeletons nearby.",
                    MessageHud.MessageType.TopLeft);
                HarmonyLog.Log($"[{Prefix}] Commanded {commanded} skeleton(s) to attack {target.name}.");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] CommandAttackAimedTarget exception: {ex}.");
            }
        }
    }
}
