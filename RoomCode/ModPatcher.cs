namespace RoomCode
{
    using System.Linq;
    using HarmonyLib;

    internal static class ModPatcher
    {
        /// <summary>
        /// Photon's GameIdAlreadyExists error code, which the game itself checks in
        /// <c>CreatingGameState.OnCreateRoomFailed</c> before retrying with a new room name.
        /// </summary>
        private const short GameIdAlreadyExists = 32766;

        private static int _roomCodeIndex;

        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NetworkRoomUtils), "GetRandomRoomCode"),
                prefix: new HarmonyMethod(typeof(ModPatcher), nameof(NetworkRoomUtils_GetRandomRoomCode_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CreatingGameState), "Enter"),
                prefix: new HarmonyMethod(typeof(ModPatcher), nameof(ResetRoomCodeIndex)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CreatingGameState), "OnCreateRoomFailed"),
                prefix: new HarmonyMethod(typeof(ModPatcher), nameof(CreatingGameState_OnCreateRoomFailed_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CreatingGameState), "OnJoinedRoom"),
                prefix: new HarmonyMethod(typeof(ModPatcher), nameof(ResetRoomCodeIndex)));
        }

        /// <summary>
        /// Proposes the current preferred room code.
        /// </summary>
        /// <remarks>
        /// The game can ask for a room code more than once while it connects, without any room being created in
        /// between. The proposed code therefore only moves to the next one when the server reports the current
        /// code as taken (see <see cref="CreatingGameState_OnCreateRoomFailed_Prefix"/>).
        /// </remarks>
        private static bool NetworkRoomUtils_GetRandomRoomCode_Prefix(ref string __result)
        {
            if (!RoomCodeBase.Enabled)
            {
                return true;
            }

            if (!RoomCodeBase.RoomCodes.Any())
            {
                return true;
            }

            if (_roomCodeIndex >= RoomCodeBase.RoomCodes.Count)
            {
                RoomCodeBase.LogInfo("All proposed room codes unavailable. Using a random room code.");
                return true;
            }

            __result = RoomCodeBase.RoomCodes.ElementAt(_roomCodeIndex);
            RoomCodeBase.LogInfo($"Proposing room code: {__result}");
            return false;
        }

        private static void CreatingGameState_OnCreateRoomFailed_Prefix(short returnCode)
        {
            if (returnCode != GameIdAlreadyExists || _roomCodeIndex >= RoomCodeBase.RoomCodes.Count)
            {
                return;
            }

            RoomCodeBase.LogInfo($"Room code {RoomCodeBase.RoomCodes.ElementAt(_roomCodeIndex)} is taken. Trying the next one.");
            ++_roomCodeIndex;
        }

        private static void ResetRoomCodeIndex()
        {
            _roomCodeIndex = 0;
        }
    }
}
