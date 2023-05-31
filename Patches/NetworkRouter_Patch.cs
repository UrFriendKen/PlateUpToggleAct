using Controllers;
using HarmonyLib;
using Kitchen;
using System.Collections.Generic;
using System.Reflection;

namespace KitchenToggleAct.Patches
{
    [HarmonyPatch]
    static class NetworkRouter_Patch
    {
        public static HashSet<int> _actHolds = new HashSet<int>();

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
                    if (!_actHolds.Contains(update.Data.User))
                    {
                        _actHolds.Add(update.Data.User);
                    }
                    else
                    {
                        _actHolds.Remove(update.Data.User);
                    }
                }
                else if (_actHolds.Contains(update.Data.User))
                {
                    update.Data.State.InteractAction = ButtonState.Held;
                }

                if (update.Data.State.GrabAction == ButtonState.Pressed)
                {
                    if (_actHolds.Contains(update.Data.User))
                    {
                        _actHolds.Remove(update.Data.User);
                    }
                }
            }
        }
    }
}
