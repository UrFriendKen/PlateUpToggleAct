using Controllers;
using HarmonyLib;
using Kitchen;
using System.Collections.Generic;
using System.Reflection;

namespace KitchenToggleAct.Patches
{
    [HarmonyPatch]
    internal static class NetworkRouter_Patch
    {
        internal static HashSet<int> ActHolds = new HashSet<int>();

        static MethodBase TargetMethod()
        {
            return typeof(NetworkRouter).GetMethod(nameof(NetworkRouter.BroadcastCommand)).MakeGenericMethod(typeof(UserInputUpdate));
        }

        static void Prefix(ref UserInputUpdate update)
        {
            if (update.SourceIdentifier.Value == InputSourceIdentifier.Identifier)
            {
                if (update.Data.State.InteractAction == ButtonState.Pressed)
                {
                    if (!ActHolds.Contains(update.Data.User))
                    {
                        ActHolds.Add(update.Data.User);
                    }
                    else
                    {
                        ActHolds.Remove(update.Data.User);
                    }
                }
                else if (ActHolds.Contains(update.Data.User))
                {
                    update.Data.State.InteractAction = ButtonState.Held;
                }

                if (update.Data.State.GrabAction == ButtonState.Pressed || update.Data.State.SecondaryAction2 == ButtonState.Pressed)
                {
                    if (ActHolds.Contains(update.Data.User))
                    {
                        ActHolds.Remove(update.Data.User);
                    }
                }
            }
        }
    }
}
