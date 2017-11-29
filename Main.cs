using System;
using System.Collections.Generic;
using System.Xml;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Server.Constant;

namespace OutfitTester
{
    public class Outfit
    {
        public Tuple<int, int>[] Components { get; set; }
        public Tuple<int, int>[] Props { get; set; }
    }

    public class Main : Script
    {
        const int MaxComponent = 12;
        const int MaxProp = 9;
        List<Outfit> MaleOutfits = new List<Outfit>();
        List<Outfit> FemaleOutfits = new List<Outfit>();

        public Main()
        {
            API.onResourceStart += OutfitTester_Init;
        }

        public void OutfitTester_Init()
        {
            if (!System.IO.File.Exists(API.getResourceFolder() + "/scriptmetadata.meta"))
            {
                API.consoleOutput("OutfitTester doesn't work without scriptmetadata.meta!");
                API.consoleOutput("Export it from \"update\\update.rpf\\common\\data\" using OpenIV.");
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(API.getResourceFolder() + "/scriptmetadata.meta");

            // 200IQ code incoming
            foreach (XmlNode node in doc.SelectNodes("/CScriptMetadata/MPOutfits/*/MPOutfitsData/Item"))
            {
                Outfit newOutfit = new Outfit
                {
                    Components = new Tuple<int, int>[MaxComponent],
                    Props = new Tuple<int, int>[MaxProp]
                };

                // Load components
                XmlNode components = node.SelectSingleNode("ComponentDrawables");
                XmlNode componentTextures = node.SelectSingleNode("ComponentTextures");

                for (int compID = 0; compID < MaxComponent; compID++)
                {
                    newOutfit.Components[compID] = new Tuple<int, int>(Convert.ToInt32(components.ChildNodes[compID].Attributes["value"].Value), Convert.ToInt32(componentTextures.ChildNodes[compID].Attributes["value"].Value));
                }

                // Load props
                XmlNode props = node.SelectSingleNode("PropIndices");
                XmlNode propTextures = node.SelectSingleNode("PropTextures");

                for (int propID = 0; propID < MaxProp; propID++)
                {
                    newOutfit.Props[propID] = new Tuple<int, int>(Convert.ToInt32(props.ChildNodes[propID].Attributes["value"].Value), Convert.ToInt32(propTextures.ChildNodes[propID].Attributes["value"].Value));
                }

                switch (node.ParentNode.ParentNode.Name)
                {
                    case "MPOutfitsDataMale":
                        MaleOutfits.Add(newOutfit);
                    break;

                    case "MPOutfitsDataFemale":
                        FemaleOutfits.Add(newOutfit);
                    break;

                    default:
                        API.consoleOutput("WTF?");
                    break;
                }
            }

            API.consoleOutput("Loaded {0} outfits for FreemodeMale01.", MaleOutfits.Count);
            API.consoleOutput("Loaded {0} outfits for FreemodeFemale01.", FemaleOutfits.Count);
        }

        [Command("outfit")]
        public void CMD_Outfit(Client player, int ID)
        {
            switch ((PedHash)player.model)
            {
                case PedHash.FreemodeMale01:
                    if (ID < 0 || ID >= MaleOutfits.Count)
                    {
                        player.sendChatMessage("Invalid outfit ID, valid IDs: 0 - " + (MaleOutfits.Count - 1) + ".");
                        return;
                    }

                    for (int i = 0; i < MaxComponent; i++)
                    {
                        player.setClothes(i, MaleOutfits[ID].Components[i].Item1, MaleOutfits[ID].Components[i].Item2);
                    }

                    for (int i = 0; i < MaxProp; i++)
                    {
                        player.clearAccessory(i);
                        player.setAccessories(i, MaleOutfits[ID].Props[i].Item1, MaleOutfits[ID].Props[i].Item2);
                    }

                break;

                case PedHash.FreemodeFemale01:
                    if (ID < 0 || ID >= FemaleOutfits.Count)
                    {
                        player.sendChatMessage("Invalid outfit ID, valid IDs: 0 - " + (FemaleOutfits.Count - 1) + ".");
                        return;
                    }

                    for (int i = 0; i < MaxComponent; i++)
                    {
                        player.setClothes(i, FemaleOutfits[ID].Components[i].Item1, FemaleOutfits[ID].Components[i].Item2);
                    }

                    for (int i = 0; i < MaxProp; i++)
                    {
                        player.clearAccessory(i);
                        player.setAccessories(i, FemaleOutfits[ID].Props[i].Item1, FemaleOutfits[ID].Props[i].Item2);
                    }

                break;

                default:
                    player.sendChatMessage("This command only works with FreemodeMale01 and FreemodeFemale01.");
                break;
            }
        }
    }
}
