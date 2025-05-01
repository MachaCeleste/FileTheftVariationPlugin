using HarmonyLib;
using MissionConfig;
using ServiceConfig;
using System.Collections.Generic;
using System;
using Util;
using FileTheftVariationPlugin;
using System.Reflection;

[HarmonyPatch]
public class PlayerMissionsPatch
{
    [HarmonyPatch(typeof(PlayerMissions), "AddStealFileMission")]
    class AddStealFileMissionPatch
    {
        static bool Prefix(PlayerMissions __instance, ref DirectMission mission, ref string idioma, ref string __result)
        {
            if (DataUtils.flavorList.Count < 4)
            {
                Plugin.Logger.LogInfo("Flavor list must have 4 or more items in it, falling back to default code.");
                return true;
            }
            List<ServerMap.TipoRed> listTipoRedComun = OS.GetListTipoRedComun();
            listTipoRedComun.Add(ServerMap.TipoRed.Bancos);
            System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
            ServerMap.TipoRed tipoRed = listTipoRedComun[random.Next(listTipoRedComun.Count)];
            Router router = ServerMap.Singleton.SpawnRouter(tipoRed, mission.GetAccessType(), null, true, "");
            Computer computer;
            string text;
            FileSystem.Archivo archivo;
            if (tipoRed == ServerMap.TipoRed.Bancos)
            {
                Servicio servicio;
                LanPcInfo servidorLan = router.GetServidorLan(ServicioID.bank_account, out servicio);
                computer = router.GetComputer(servidorLan.localIp, false);
                text = "/server/accountsd";
                archivo = computer.GetFileSystem().GetArchivo(text, true, 0);
                archivo.ID = Guid.NewGuid().ToString();
            }
            else
            {
                LanPcInfo randomComputer = router.GetRandomComputer(false);
                computer = router.GetComputer(randomComputer.localIp, false);
                Computer.User randomUser = computer.GetRandomUser(null);
                string[] array = [.. DataUtils.flavorList];
                string text2 = array[random.Next(array.Length)] + ".bin";
                text = "/home/" + randomUser.nombreUsuario + "/" + text2;
                archivo = new FileSystem.Archivo(text2, randomUser.nombreUsuario, true)
                {
                    ID = Guid.NewGuid().ToString()
                };
                computer.GetFileSystem().AddFile(archivo, "/home/" + randomUser.nombreUsuario);
            }
            archivo.SetDefaultContent(true);
            string result = XmlGlobal.GetTexto(mission.contentID, idioma, false).Replace("$PUBLIC_IP", computer.GetPublicIP().BoldString()).Replace("$LOCAL_IP", computer.GetLocalIP().BoldString()).Replace("$FILE_PATH", text.BoldString());
            FieldInfo fieldInfo = AccessTools.Field(typeof(PlayerMissions), "missions");
            var missions = fieldInfo.GetValue(__instance) as Dictionary<string, ActiveMission>;
            missions.Add(mission.ID, new StealFileMission(archivo.ID, computer.GetID(), mission.rep));
            Database.Singleton.SyncFileSystemDB(computer, false);
            __result = result;
            return false;
        }
    }
}