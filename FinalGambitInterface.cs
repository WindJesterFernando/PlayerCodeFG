using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Final_Gambit_Player_Code
{
    public class FinalGambitInterface
    {
        public FinalGambitInterface()
        {
        }
    }
    


    public class PartyAIManager
    {

        const int CoreStats = 1;
        const int Perk = 2;
        const int StatusEffect = 3;
        const int PassiveAbility = 4;
        const int PartyInventoryItem = 5;
        const int Ability = 6;
        const int FoePartyInfoStart = 7;

        const int CharacterWithInitiative = 8;
        const int PossibleAction = 9;
        const int ActionLegality = 10;
        const int RecordOfBattleAction = 11;
        const int IsFistStateSentForNewBattle = 12;

        public static bool actionFileHasBeenWriten;

        public PartyAIManager()
        {

            FinalGambit.BattleActionID.Init();
            FinalGambit.CharacterClassID.Init();
            FinalGambit.PassiveAbilityID.Init();
            FinalGambit.StatusEffectID.Init();
            FinalGambit.ItemID.Init();
            FinalGambit.BattleActionID.Init();
            FinalGambit.PerkID.Init();
            FinalGambit.PerkTreeID.Init();
            FinalGambit.recordOfBattleActions = new LinkedList<RecordOfBattleAction>();


            if (File.Exists(TransferPath.transferPath + "BattleReport.txt") && File.Exists(TransferPath.transferPath + "Flag.txt"))
            {
                if (!File.Exists(TransferPath.transferPath + "BattleReport.txt"))
                {
                    FileStream f = File.Open(TransferPath.transferPath + "BattleReport.txt", FileMode.Open);
                    StreamReader sr = new StreamReader(f);

                    string data;
                    while ((data = sr.ReadLine()) != null)
                    {
                        string[] processing = data.Split(",");
                        int identifier = Int32.Parse(processing[0]);

                        FinalGambit.recordOfBattleActions.AddLast(new RecordOfBattleAction(Int32.Parse(processing[1]), Int32.Parse(processing[2]), Int32.Parse(processing[3]), Int32.Parse(processing[4])));
                    }
                    sr.Close();
                    f.Close();
                }
            }

            while (true)
            {

                if (File.Exists(TransferPath.transferPath + "Error.txt") && File.Exists(TransferPath.transferPath + "Flag.txt"))
                {
                    FileStream f = File.Open(TransferPath.transferPath + "Error.txt", FileMode.Open);
                    StreamReader sr = new StreamReader(f);

                    string data;
                    while ((data = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("Error: " + data);
                    }

                    sr.Close();
                    f.Close();

                    File.Delete(TransferPath.transferPath + "Error.txt");

                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();


                }
                if (File.Exists(TransferPath.transferPath + "BattleState.txt") && File.Exists(TransferPath.transferPath + "Flag.txt"))
                {
                    ReadBattleStateSerializationFile();
                    File.Delete(TransferPath.transferPath + "BattleState.txt");
                    File.Delete(TransferPath.transferPath + "Flag.txt");

                    PartyAIManager.actionFileHasBeenWriten = false;
                    PartyAI.ProcessAI();
                }

                Thread.Sleep(1000);
            }
        }

        private void ReadBattleStateSerializationFile()
        {

            FinalGambit.BattleState = new BattleState();


            FileStream f = File.OpenRead(TransferPath.transferPath + "BattleState.txt");

            StreamReader sr = new StreamReader(f);

            bool isReadingForParty = true;
            PartyCharacter lastPC = null;
            LegalActionAndRequirments lastAction = null;

            // Console.WriteLine("starting reading ");

            string data;
            while ((data = sr.ReadLine()) != null)
            {
                //Console.WriteLine("reading " + data);
                string[] processing = data.Split(",");
                int identifier = Int32.Parse(processing[0]);

                if (identifier == CoreStats)
                {
                    lastPC = new PartyCharacter(Int32.Parse(processing[1]), Int32.Parse(processing[2]), Int32.Parse(processing[3]), Int32.Parse(processing[4]), Int32.Parse(processing[5]), float.Parse(processing[6]), float.Parse(processing[7]), Int32.Parse(processing[8]), Int32.Parse(processing[9]), Int32.Parse(processing[10]), Int32.Parse(processing[11]));
                    if (isReadingForParty)
                        FinalGambit.BattleState.partyCharacters.AddLast(lastPC);
                    else
                        FinalGambit.BattleState.foeCharacters.AddLast(lastPC);
                }
                else if (identifier == FoePartyInfoStart)
                {
                    isReadingForParty = false;
                }
                else if (identifier == Perk)
                {
                    lastPC.perkIDs.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == StatusEffect)
                {
                    lastPC.statusEffects.AddLast(new StatusEffect(Int32.Parse(processing[1]), Int32.Parse(processing[2])));
                }
                else if (identifier == PassiveAbility)
                {
                    lastPC.passiveAbilities.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == Ability)
                {
                    lastPC.battleActions.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == PartyInventoryItem)
                {
                    if (isReadingForParty)
                        FinalGambit.BattleState.partyInventory.AddLast(new InventoryItem(Int32.Parse(processing[1]), Int32.Parse(processing[2])));
                    else
                        FinalGambit.BattleState.foeInventory.AddLast(new InventoryItem(Int32.Parse(processing[1]), Int32.Parse(processing[2])));
                }
                else if (identifier == CharacterWithInitiative)
                {
                    FinalGambit.characterWithInitiative = FinalGambit.GetCharacterFromCharacterIndex(Int32.Parse(processing[1]));
                }
                else if (identifier == PossibleAction)
                {
                    lastAction = new LegalActionAndRequirments(Int32.Parse(processing[1]));
                    FinalGambit.BattleState.legalActions.AddLast(lastAction);
                }
                else if (identifier == ActionLegality)
                {
                    //Console.WriteLine("Adding legality " + Int32.Parse(processing[1]) + " to " + FinalGambit.BattleActionID.lookUp[lastAction.actionID]);
                    lastAction.legalities.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == RecordOfBattleAction)
                {
                    FinalGambit.recordOfBattleActions.AddLast(new RecordOfBattleAction(Int32.Parse(processing[1]), Int32.Parse(processing[2]), Int32.Parse(processing[3]), Int32.Parse(processing[4])));

                    FileStream f2;
                    if (!File.Exists(TransferPath.transferPath + "BattleReport.txt"))
                    {
                        //Console.WriteLine("report create");
                        f2 = File.Create(TransferPath.transferPath + "BattleReport.txt");
                    }
                    else
                    {
                        //Console.WriteLine("report append");
                        f2 = File.Open(TransferPath.transferPath + "BattleReport.txt", FileMode.Append);
                    }

                    StreamWriter sw = new StreamWriter(f2);

                    //Console.WriteLine("report write = " + data);
                    sw.WriteLine("" + data);
                    sw.Close();

                    f2.Close();
                    
                }
                else if (identifier == IsFistStateSentForNewBattle)
                {
                    FinalGambit.recordOfBattleActions = new LinkedList<RecordOfBattleAction>();
                    File.Delete(TransferPath.transferPath + "BattleReport.txt");

                }

            }
            sr.Close();

            f.Close();
        }

    }

    public class BattleState
    {
        public LinkedList<PartyCharacter> partyCharacters;
        public LinkedList<PartyCharacter> foeCharacters;

        public LinkedList<InventoryItem> partyInventory;
        public LinkedList<InventoryItem> foeInventory;

        public PartyCharacter characterWithInitiative;
        public LinkedList<LegalActionAndRequirments> legalActions;


        public BattleState()
        {
            partyCharacters = new LinkedList<PartyCharacter>();
            foeCharacters = new LinkedList<PartyCharacter>();
            partyInventory = new LinkedList<InventoryItem>();
            foeInventory = new LinkedList<InventoryItem>();

            legalActions = new LinkedList<LegalActionAndRequirments>();
            characterWithInitiative = null;
        }
    }

    public class PartyCharacter
    {
        public int classID;

        public int hp, maxHP, mp, maxMP, defense, magicDefense, attack, magicAttack;
        public float activeTimeBattleBarFill, speed;

        public LinkedList<int> battleActions;

        public LinkedList<int> perkIDs;
        public LinkedList<StatusEffect> statusEffects;

        public LinkedList<int> passiveAbilities;

        public PartyCharacter(int ClassID, int HP, int MaxHP, int MP, int MaxMP, float ActiveTimeBattleBarFill, float Speed, int Defense, int MagicDefense, int Attack, int MagicAttack)
        {
            classID = ClassID;

            hp = HP;
            mp = MP;
            maxHP = MaxHP;
            maxMP = MaxMP;
            activeTimeBattleBarFill = ActiveTimeBattleBarFill;
            speed = Speed;
            defense = Defense;
            magicDefense = MagicDefense;
            attack = Attack;
            magicAttack = MagicAttack;

            battleActions = new LinkedList<int>();
            perkIDs = new LinkedList<int>();
            passiveAbilities = new LinkedList<int>();
            statusEffects = new LinkedList<StatusEffect>();
        }
    }

    public class InventoryItem
    {
        public int id;
        public int count;

        public InventoryItem(int ID, int Count)
        {
            id = ID;
            count = Count;
        }

    }

    public class StatusEffect
    {
        public int id;
        public int duration;

        public StatusEffect(int ID, int Duration)
        {
            id = ID;
            duration = Duration;
        }
    }

    public class LegalActionAndRequirments
    {
        public int actionID;
        public LinkedList<int> legalities;

        public LegalActionAndRequirments(int ActionID)
        {
            actionID = ActionID;
            legalities = new LinkedList<int>();
        }
    }

    static public class LegalTargetIdentifiers
    {
        public const int NoTargetRequired = 1;
        public const int TargetAlly = 2;
        public const int TargetFoe = 3;
        public const int TargetSlain = 4;

    }

    public class RecordOfBattleAction
    {
        public PartyCharacter characterWithInitiative;
        public PartyCharacter characterBeingTargeted;
        public PartyCharacter characterThatPerformedCover;
        public int battleActionID;

        public RecordOfBattleAction(int CharacterWithInitiative, int CharacterBeingTargeted, int CharacterThatPerformedCover, int BattleActionID)
        {
            characterWithInitiative = FinalGambit.GetCharacterFromCharacterIndex(CharacterWithInitiative);
            characterBeingTargeted = FinalGambit.GetCharacterFromCharacterIndex(CharacterBeingTargeted);
            characterThatPerformedCover = FinalGambit.GetCharacterFromCharacterIndex(CharacterThatPerformedCover);
            battleActionID = BattleActionID;
        }
    }

    public static class FinalGambit
    {
        public static BattleState BattleState;
        public static PartyCharacter characterWithInitiative;
        public static LinkedList<RecordOfBattleAction> recordOfBattleActions;

        static public void PerformBattleAction(int battleActionID, PartyCharacter target)
        {

            if (PartyAIManager.actionFileHasBeenWriten)
            {
                Console.WriteLine("PerformBattleAction has already been called!");
                return;
            }

            //if (AreBattleActionAndTargetLegal(battleActionID, target))
            {
                FileStream f = File.Create(TransferPath.transferPath + "Action.txt");

                StreamWriter sw = new StreamWriter(f);

                if (target == null)
                    Console.WriteLine("Target is null!");

                int targetIndex = GetCharacterIndexFromCharacter(target);

                //Console.WriteLine("writing " + battleActionID + "," + targetIndex);

                sw.WriteLine(battleActionID + "," + targetIndex);

                sw.Close();
                f.Close();
                f = File.Create(TransferPath.transferPath + "Flag.txt");
                f.Close();

                PartyAIManager.actionFileHasBeenWriten = true;
            }
        }

        static public int GetCharacterIndexFromCharacter(PartyCharacter pc)
        {
            int count = 0;
            if (FinalGambit.BattleState.foeCharacters.Contains(pc))
            {
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.foeCharacters)
                {
                    count++;
                    if (pc2 == pc)
                        return -count;
                }
            }
            else if (FinalGambit.BattleState.partyCharacters.Contains(pc))
            {
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.partyCharacters)
                {
                    count++;
                    if (pc2 == pc)
                        return count;
                }
            }

            return 0;
        }

        static public PartyCharacter GetCharacterFromCharacterIndex(int index)
        {
            int count = 0;
            if (index < 0)
            {
                index = -index;
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.foeCharacters)
                {
                    count++;
                    if (count == index)
                        return pc2;
                }
            }
            else if (index > 0)
            {
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.partyCharacters)
                {
                    count++;
                    if (count == index)
                        return pc2;
                }
            }

            return null;
        }

        static public bool AreBattleActionAndTargetLegal(int battleActionID, PartyCharacter target)
        {

            bool isLegal = true;

            LegalActionAndRequirments actionAndLegalReqs = null;

            foreach (LegalActionAndRequirments legalAction in FinalGambit.BattleState.legalActions)
            {
                if (legalAction.actionID == battleActionID)
                {
                    actionAndLegalReqs = legalAction;
                    break;
                }
            }

            if (actionAndLegalReqs == null)
            {
                isLegal = false;
                Console.WriteLine(BattleActionID.lookUp[battleActionID] + " is not in the list of possible actions!");
            }

            if (actionAndLegalReqs != null)
            {
                if (target == null && !actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.NoTargetRequired))
                {
                    Console.WriteLine(BattleActionID.lookUp[battleActionID] + " requires a target!");
                    isLegal = false;
                }

                if (target != null)
                {
                    if (target.hp > 0)
                    {
                        if (actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetSlain))
                        {
                            Console.WriteLine(BattleActionID.lookUp[battleActionID] + " requires a slain target!");
                            isLegal = false;
                        }
                    }
                    else
                    {
                        if (!actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetSlain))
                        {
                            Console.WriteLine(BattleActionID.lookUp[battleActionID] + " cannot target a slain character!");
                            isLegal = false;
                        }
                    }


                    int ind = GetCharacterIndexFromCharacter(target);

                    //Console.WriteLine("ind = " + ind + "  " + actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetFoe) + "    " + actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetAlly));

                    if (ind > 0)
                    {
                        if (actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetFoe))
                        {
                            Console.WriteLine(BattleActionID.lookUp[battleActionID] + " cannot target an ally, must target a foe!");
                            isLegal = false;
                        }
                    }
                    else if (ind < 0)
                    {
                        if (actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetAlly))
                        {
                            Console.WriteLine(BattleActionID.lookUp[battleActionID] + " cannot target a foe, must target an ally!");
                            isLegal = false;
                        }
                    }
                }

            }

            return isLegal;
        }

        static public class CharacterClassID
        {
            public const int Fighter = 0;
            public const int BlackMage = 1;
            public const int WhiteMage = 2;
            public const int Thief = 3;

            public const int Monk = 4;
            public const int RedMage = 5;

            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(CharacterClassID.Fighter, "Fighter");
                lookUp.Add(CharacterClassID.BlackMage, "Black Mage");
                lookUp.Add(CharacterClassID.WhiteMage, "White Mage");
                lookUp.Add(CharacterClassID.Thief, "Thief");
                lookUp.Add(CharacterClassID.Monk, "Monk");
                lookUp.Add(CharacterClassID.RedMage, "Red Mage");

                // foreach(string name in lookUp.Values)
                // {
                //     Debug.Log("** " + name);
                // }
            }

        }

        static public class BattleActionID
        {
            public const int NoValue = -1;
            public const int Attack = 0;
            public const int Steal = 1;
            //public const int PoisonStrike = 2;
            //public const int Fire = 3;
            //public const int Ice = 4;
            //public const int MagicMissile = 5;
            //public const int Cure = 6;
            //public const int PatHead = 7;
            //public const int Life = 8;
            // public const int Potion = 9;
            // public const int Ether = 10;
            // public const int Elixer = 11;
            // public const int PhoenixDown = 12;

            public const int Haste = 13;
            public const int Slow = 14;
            public const int Regen = 15;

            public const int Mini = 16;
            public const int Protect = 17;
            public const int Shell = 18;
            public const int Faith = 19;
            public const int Brave = 20;
            public const int Debrave = 21;
            public const int Defaith = 22;
            public const int Deprotect = 23;
            public const int Deshell = 24;




            public const int Silence = 25;
            public const int Beserk = 26;
            public const int Petrify = 27;
            //public const int Petrified = 28;
            public const int Bubble = 29;
            public const int AutoLife = 30;
            public const int Doom = 31;

            //public const int Lightning = 32;
            public const int Esuna = 33;
            public const int Chakra = 34;


            public const int Blizzard = 101;
            public const int Blizzara = 102;
            public const int Blizzaga = 103;
            public const int Fire = 104;
            public const int Fira = 105;
            public const int Firaga = 106;
            public const int Thunder = 107;
            public const int Thundara = 108;
            public const int Thundaga = 109;

            public const int BlizzardAll = 111;
            public const int BlizzaraAll = 112;
            public const int BlizzagaAll = 113;
            public const int FireAll = 114;
            public const int FiraAll = 115;
            public const int FiragaAll = 116;
            public const int ThunderAll = 117;
            public const int ThundaraAll = 118;
            public const int ThundagaAll = 119;


            public const int Cure = 120;
            public const int Cura = 121;
            public const int Curaga = 122;
            public const int Life = 123;
            public const int Lifa = 124;
            public const int Lifaga = 125;



            public const int Meteor = 130;
            public const int Flare = 131;
            public const int Dispel = 132;

            public const int PoisonStrike = 141;
            public const int DispelStrike = 142;
            public const int ManaBurnStrike = 143;
            public const int FlurryOfBlows = 144;


            public const int Potion = 200;
            public const int HighPotion = 201;
            public const int Ether = 202;
            public const int HighEther = 203;
            public const int Elixer = 204;
            public const int MegaElixer = 205;
            public const int PhoenixDown = 206;
            public const int EchoHerbs = 207;
            public const int Antidote = 208;
            public const int GysahlGreens = 209;
            public const int GoldenNeedle = 210;
            public const int Remedy = 211;



            public const int CraftPotion = 300;
            public const int CraftHighPotion = 301;
            public const int CraftEther = 3023;
            public const int CraftHighEther = 303;
            public const int CraftElixer = 304;
            public const int CraftMegaElixer = 305;
            public const int CraftPhoenixDown = 306;
            public const int CraftEchoHerbs = 307;
            public const int CraftAntidote = 308;
            public const int CraftGysahlGreens = 309;
            public const int CraftGoldenNeedle = 310;
            public const int CraftRemedy = 311;



            static public Dictionary<int, string> lookUp;
            static List<int> offensiveIDs;
            //static List<int> itemIDs;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(BattleActionID.NoValue, "");
                lookUp.Add(BattleActionID.Attack, "Attack");
                lookUp.Add(BattleActionID.Steal, "Steal");

                // lookUp.Add(BattleActionID.Fire, "Fire");
                // lookUp.Add(BattleActionID.Ice, "Ice");
                // lookUp.Add(BattleActionID.MagicMissile, "Magic Missile");

                // lookUp.Add(BattleActionID.PatHead, "Pat Head");


                lookUp.Add(BattleActionID.Haste, "Haste");
                lookUp.Add(BattleActionID.Slow, "Slow");

                lookUp.Add(BattleActionID.Regen, "Regen");

                lookUp.Add(BattleActionID.Mini, "Mini");
                lookUp.Add(BattleActionID.Protect, "Protect");
                lookUp.Add(BattleActionID.Shell, "Shell");
                lookUp.Add(BattleActionID.Faith, "Faith");
                lookUp.Add(BattleActionID.Brave, "Brave");
                lookUp.Add(BattleActionID.Debrave, "Debrave");
                lookUp.Add(BattleActionID.Defaith, "Defaith");
                lookUp.Add(BattleActionID.Deprotect, "Deprotect");
                lookUp.Add(BattleActionID.Deshell, "Deshell");


                lookUp.Add(BattleActionID.Silence, "Silence");
                lookUp.Add(BattleActionID.Beserk, "Beserk");
                lookUp.Add(BattleActionID.Petrify, "Petrify");
                lookUp.Add(BattleActionID.Bubble, "Bubble");
                lookUp.Add(BattleActionID.AutoLife, "Auto Life");
                lookUp.Add(BattleActionID.Doom, "Doom");

                // lookUp.Add(BattleActionID.Lightning, "Lightning");
                lookUp.Add(BattleActionID.Esuna, "Esuna");
                lookUp.Add(BattleActionID.Chakra, "Chakra");





                // lookUp.Add(BattleActionID.Cure, "Cure");
                // lookUp.Add(BattleActionID.Life, "Life");
                // lookUp.Add(BattleActionID.Potion, "Potion");
                // lookUp.Add(BattleActionID.Ether, "Ether");
                // lookUp.Add(BattleActionID.Elixer, "Elixer");
                // lookUp.Add(BattleActionID.PhoenixDown, "PhoenixDown");



                // lookUp.Add(BattleActionID.PoisonStrike, "Poison Strike");


                lookUp.Add(BattleActionID.Blizzard, "Blizzard");
                lookUp.Add(BattleActionID.Blizzara, "Blizzara");
                lookUp.Add(BattleActionID.Blizzaga, "Blizzaga");
                lookUp.Add(BattleActionID.Fire, "Fire");
                lookUp.Add(BattleActionID.Fira, "Fira");
                lookUp.Add(BattleActionID.Firaga, "Firaga");
                lookUp.Add(BattleActionID.Thunder, "Thunder");
                lookUp.Add(BattleActionID.Thundara, "Thundara");
                lookUp.Add(BattleActionID.Thundaga, "Thundaga");

                lookUp.Add(BattleActionID.BlizzardAll, "Blizzard All");
                lookUp.Add(BattleActionID.BlizzaraAll, "Blizzara All");
                lookUp.Add(BattleActionID.BlizzagaAll, "Blizzaga All");
                lookUp.Add(BattleActionID.FireAll, "Fire All");
                lookUp.Add(BattleActionID.FiraAll, "Fira All");
                lookUp.Add(BattleActionID.FiragaAll, "Firaga All");
                lookUp.Add(BattleActionID.ThunderAll, "Thunder All");
                lookUp.Add(BattleActionID.ThundaraAll, "Thundara All");
                lookUp.Add(BattleActionID.ThundagaAll, "Thundaga All");

                lookUp.Add(BattleActionID.Meteor, "Meteor");
                lookUp.Add(BattleActionID.Flare, "Flare");
                lookUp.Add(BattleActionID.Dispel, "Dispel");




                lookUp.Add(BattleActionID.Cure, "Cure");
                lookUp.Add(BattleActionID.Cura, "Cura");
                lookUp.Add(BattleActionID.Curaga, "Curaga");

                lookUp.Add(BattleActionID.Life, "Life");
                lookUp.Add(BattleActionID.Lifa, "Lifa");
                lookUp.Add(BattleActionID.Lifaga, "Lifaga");





                lookUp.Add(BattleActionID.PoisonStrike, "Poison Strike");
                lookUp.Add(BattleActionID.DispelStrike, "Dispel Strike");
                lookUp.Add(BattleActionID.ManaBurnStrike, "Mana Burn Strike");
                lookUp.Add(BattleActionID.FlurryOfBlows, "Flurry Of Blows");




                lookUp.Add(BattleActionID.Potion, "Potion");
                lookUp.Add(BattleActionID.HighPotion, "High Potion");
                lookUp.Add(BattleActionID.Ether, "Ether");
                lookUp.Add(BattleActionID.HighEther, "High Ether");
                lookUp.Add(BattleActionID.Elixer, "Elixer");
                lookUp.Add(BattleActionID.MegaElixer, "Mega Elixer");
                lookUp.Add(BattleActionID.PhoenixDown, "Phoenix Down");
                lookUp.Add(BattleActionID.EchoHerbs, "Echo Herbs");
                lookUp.Add(BattleActionID.Antidote, "Antidote");
                lookUp.Add(BattleActionID.GysahlGreens, "Gysahl Greens");
                lookUp.Add(BattleActionID.GoldenNeedle, "Golden Needle");
                lookUp.Add(BattleActionID.Remedy, "Remedy");




                lookUp.Add(BattleActionID.CraftPotion, "Craft Potion");
                lookUp.Add(BattleActionID.CraftHighPotion, "Craft High Potion");
                lookUp.Add(BattleActionID.CraftEther, "Craft Ether");
                lookUp.Add(BattleActionID.CraftHighEther, "Craft High Ether");
                lookUp.Add(BattleActionID.CraftElixer, "Craft Elixer");
                lookUp.Add(BattleActionID.CraftMegaElixer, "Craft Mega Elixer");
                lookUp.Add(BattleActionID.CraftPhoenixDown, "Craft Phoenix Down");
                lookUp.Add(BattleActionID.CraftEchoHerbs, "Craft Echo Herbs");
                lookUp.Add(BattleActionID.CraftAntidote, "Craft Antidote");
                lookUp.Add(BattleActionID.CraftGysahlGreens, "Craft Gysahl Greens");
                lookUp.Add(BattleActionID.CraftGoldenNeedle, "Craft Golden Needle");
                lookUp.Add(BattleActionID.CraftRemedy, "Craft Remedy");




                offensiveIDs = new List<int>(){
            BattleActionID.Attack,
            BattleActionID.Steal,
            // BattleActionID.PoisonStrike,
            // BattleActionID.Fire,
            // BattleActionID.Ice,
            // BattleActionID.MagicMissile,
            BattleActionID.Slow,
            BattleActionID.Mini,
            BattleActionID.Debrave,
            BattleActionID.Defaith,
            BattleActionID.Deprotect,
            BattleActionID.Deshell,
            BattleActionID.Silence,
            BattleActionID.Beserk,
            BattleActionID.Petrify,
            BattleActionID.Doom,
            // BattleActionID.Lightning,

            BattleActionID.Blizzard,
            BattleActionID.Blizzara,
            BattleActionID.Blizzaga,
            BattleActionID.Fire,
            BattleActionID.Fira,
            BattleActionID.Firaga,
            BattleActionID.Thunder,
            BattleActionID.Thundara,
            BattleActionID.Thundaga,

            BattleActionID.BlizzardAll,
            BattleActionID.BlizzaraAll,
            BattleActionID.BlizzagaAll,
            BattleActionID.FireAll,
            BattleActionID.FiraAll,
            BattleActionID.FiragaAll,
            BattleActionID.ThunderAll,
            BattleActionID.ThundaraAll,
            BattleActionID.ThundagaAll,


            BattleActionID.Meteor,
            BattleActionID.Flare,
            BattleActionID.Dispel,
            BattleActionID.PoisonStrike,
            BattleActionID.DispelStrike,
            BattleActionID.ManaBurnStrike,
            BattleActionID.FlurryOfBlows,


            };

                // itemIDs = new List<int>(){
                //         BattleActionID.Potion,
                //         BattleActionID.Ether,
                //         BattleActionID.Elixer,
                //         BattleActionID.PhoenixDown
                // };
            }

            static public bool IsOffensiveID(int battleActionID)
            {
                return offensiveIDs.Contains(battleActionID);
            }

            static public bool IsBattleActionItem(int battleActionID)
            {
                string battleActionName = lookUp[battleActionID];
                foreach (int i in ItemID.lookUp.Keys)
                {
                    if (ItemID.lookUp[i] == battleActionName)
                        return true;
                }

                return false;
            }
        }

        static public class ItemID
        {
            // public const int Potion = 0;
            // public const int Ether = 1;
            // public const int Elixer = 2;
            // public const int PhoenixDown = 3;

            public const int Potion = 200;
            public const int HighPotion = 201;
            public const int Ether = 202;
            public const int HighEther = 203;
            public const int Elixer = 204;
            public const int MegaElixer = 205;
            public const int PhoenixDown = 206;
            public const int EchoHerbs = 207;
            public const int Antidote = 208;
            public const int GysahlGreens = 209;
            public const int GoldenNeedle = 210;
            public const int Remedy = 211;

            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                // lookUp.Add(ItemID.Potion, "Potion");
                // lookUp.Add(ItemID.Ether, "Ether");
                // lookUp.Add(ItemID.Elixer, "Elixer");
                // lookUp.Add(ItemID.PhoenixDown, "Phoenix Down");

                lookUp.Add(ItemID.Potion, "Potion");
                lookUp.Add(ItemID.HighPotion, "High Potion");
                lookUp.Add(ItemID.Ether, "Ether");
                lookUp.Add(ItemID.HighEther, "High Ether");
                lookUp.Add(ItemID.Elixer, "Elixer");
                lookUp.Add(ItemID.MegaElixer, "Mega Elixer");
                lookUp.Add(ItemID.PhoenixDown, "Phoenix Down");
                lookUp.Add(ItemID.EchoHerbs, "Echo Herbs");
                lookUp.Add(ItemID.Antidote, "Antidote");
                lookUp.Add(ItemID.GysahlGreens, "Gysahl Greens");
                lookUp.Add(ItemID.GoldenNeedle, "Golden Needle");
                lookUp.Add(ItemID.Remedy, "Remedy");

            }

        }

        static public class PassiveAbilityID
        {
            public const int Cover = 1;
            // public const int Counter = 2;
            // public const int AutoPotion = 3;

            public const int FireWeakness = 2;
            public const int IceWeakness = 3;
            public const int LightningWeakness = 4;
            public const int EarthWeakness = 5;
            public const int WaterWeakness = 6;
            public const int WindWeakness = 7;
            public const int DarkWeakness = 8;
            public const int HolyWeakness = 9;

            public const int FireResistance = 10;
            public const int IceResistance = 11;
            public const int LightningResistance = 12;
            public const int EarthResistance = 13;
            public const int WaterResistance = 14;
            public const int WindResistance = 15;
            public const int DarkResistance = 16;
            public const int HolyResistance = 17;


            public const int AutoPotion = 18;
            public const int ItemJockey = 19;
            public const int SneakAttack = 20;


            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(PassiveAbilityID.Cover, "Cover");
                lookUp.Add(PassiveAbilityID.FireWeakness, "Fire Weakness");
                lookUp.Add(PassiveAbilityID.IceWeakness, "Ice Weakness");
                lookUp.Add(PassiveAbilityID.LightningWeakness, "Lightning Weakness");
                lookUp.Add(PassiveAbilityID.EarthWeakness, "Earth Weakness");
                lookUp.Add(PassiveAbilityID.WaterWeakness, "Water Weakness");
                lookUp.Add(PassiveAbilityID.WindWeakness, "Wind Weakness");
                lookUp.Add(PassiveAbilityID.DarkWeakness, "Dark Weakness");
                lookUp.Add(PassiveAbilityID.HolyWeakness, "Holy Weakness");
                lookUp.Add(PassiveAbilityID.FireResistance, "Fire Resistance");
                lookUp.Add(PassiveAbilityID.IceResistance, "Ice Resistance");
                lookUp.Add(PassiveAbilityID.LightningResistance, "Lightning Resistance");
                lookUp.Add(PassiveAbilityID.EarthResistance, "Earth Resistance");
                lookUp.Add(PassiveAbilityID.WaterResistance, "Water Resistance");
                lookUp.Add(PassiveAbilityID.WindResistance, "Wind Resistance");
                lookUp.Add(PassiveAbilityID.DarkResistance, "Dark Resistance");
                lookUp.Add(PassiveAbilityID.HolyResistance, "Holy Resistance");

                lookUp.Add(PassiveAbilityID.AutoPotion, "Auto Potion");
                lookUp.Add(PassiveAbilityID.ItemJockey, "Item Jockey");
                lookUp.Add(PassiveAbilityID.SneakAttack, "Sneak Attack");



            }


        }

        static public class StatusEffectID
        {
            public const int Poison = 1;
            public const int Regen = 3;

            public const int Haste = 4;
            public const int Slow = 5;

            public const int Mini = 6;
            public const int Protect = 7;
            public const int Shell = 8;
            public const int Faith = 9;
            public const int Brave = 10;
            public const int Debrave = 11;
            public const int Defaith = 12;
            public const int Deprotect = 13;
            public const int Deshell = 14;

            public const int Silence = 15;
            public const int Beserk = 16;
            public const int Petrifying = 17;
            public const int Petrified = 18;
            public const int Bubble = 19;
            public const int AutoLife = 20;
            public const int Doom = 21;


            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(StatusEffectID.Poison, "Poison");
                lookUp.Add(StatusEffectID.Regen, "Regen");
                lookUp.Add(StatusEffectID.Haste, "Haste");
                lookUp.Add(StatusEffectID.Slow, "Slow");
                //lookUp.Add(StatusEffectID.Mini, "Mini");
                lookUp.Add(StatusEffectID.Protect, "Protect");
                lookUp.Add(StatusEffectID.Shell, "Shell");
                lookUp.Add(StatusEffectID.Faith, "Faith");
                lookUp.Add(StatusEffectID.Brave, "Brave");
                lookUp.Add(StatusEffectID.Debrave, "Debrave");
                lookUp.Add(StatusEffectID.Defaith, "Defaith");
                lookUp.Add(StatusEffectID.Deprotect, "Deprotect");
                lookUp.Add(StatusEffectID.Deshell, "Deshell");
                lookUp.Add(StatusEffectID.Silence, "Silence");
                lookUp.Add(StatusEffectID.Beserk, "Beserk");
                lookUp.Add(StatusEffectID.Petrifying, "Petrifying");
                lookUp.Add(StatusEffectID.Petrified, "Petrified");
                lookUp.Add(StatusEffectID.Bubble, "Bubble");
                lookUp.Add(StatusEffectID.AutoLife, "AutoLife");



            }

        }

        static public class StatusEffectsParams
        {
            public const int HasteDuration = 5;
            public const int SlowDuration = 5;

            public const float HasteSpeedMod = 1.5f;
            public const float SlowSpeedMod = 0.5f;

            public const int PoisonDuration = 5;
            public const int PoisonTickDamage = -5;

            public const int RegenDuration = 5;
            public const int RegenTickHeal = 5;



            public const int ProtectDuration = 5;
            public const float ProtectDefMod = 0.5f;

            public const int DeprotectDuration = 5;
            public const float DeprotectDefMod = -0.5f;

            public const int ShellDuration = 5;
            public const float ShellMagDefMod = 0.5f;

            public const int DeshellDuration = 5;
            public const float DeshellMagDefMod = -0.5f;

            public const int BraveDuration = 5;
            public const float BraveAttackMod = 0.25f;
            public const float BraveDefMod = 0.25f;

            public const int DebraveDuration = 5;
            public const float DebraveAttackMod = -0.25f;
            public const float DebraveDefMod = -0.25f;

            public const int FaithDuration = 5;
            public const float FaithMagMod = 0.25f;
            public const float FaithMagDefMod = 0.25f;

            public const int DefaithDuration = 5;
            public const float DefaithMagAttackMod = -0.25f;
            public const float DefaithMagDefMod = -0.25f;

            public const int MiniDuration = 5;
            public const float MiniDefMod = -0.25f;
            public const float MiniAttackMod = -0.25f;


            public const int PetrificationTime = 3;
            public const int DoomTime = 3;


            public const int SilenceDuration = 5;
            public const int BeserkDuration = 5;
            public const int BubbleDuration = 10;
            public const int AutoLifeDuration = 9999;


        }
        static public class PassiveEffectParams
        {
            public const float ElementalWeaknessDamageMod = 1.5f;
            public const float ElementalResistanceDamageMod = 0.5f;

            public const float ItemJockeyRefillATB = 0.5f;
        }
        static public class BattleActionParams
        {

            // public const int PotionHealAmount = 50;
            // public const int EtherMPHealAmount = 25;
            // public const int ElixerHealAmount = 50;
            // public const int ElixerMPHealAmount = 25;
            // public const int PhoenixDownHealAmount = 25;

            public const float AttackMod = 1f;

            // public const float FireMod = 1f;
            // public const floatat CureMod = 1f;


            public const float BlizzardMod = 1.0f;
            public const float BlizzaraMod = 1.25f;
            public const float BlizzagaMod = 1.5f;
            public const float FireMod = 1.0f;
            public const float FiraMod = 1.25f;
            public const float FiragaMod = 1.5f;
            public const float ThunderMod = 1.0f;
            public const float ThundaraMod = 1.25f;
            public const float ThundagaMod = 1.5f;
            public const float BlizzardAllMod = 0.5f;
            public const float BlizzaraAllMod = 0.75f;
            public const float BlizzagaAllMod = 0.1f;
            public const float FireAllMod = 0.5f;
            public const float FiraAllMod = 0.75f;
            public const float FiragaAllMod = 1.0f;
            public const float ThunderAllMod = 0.5f;
            public const float ThundaraAllMod = 0.75f;
            public const float ThundagaAllMod = 1.0f;
            public const float MeteorMod = 1.5f;
            public const float FlareMod = 2.0f;


            public const float CureMod = 1.25f;
            public const float CuraMod = 1.5f;
            public const float CuragaMod = 1.75f;
            public const float LifeMod = 0.5f;
            public const float LifaMod = 0.75f;
            public const float LifagaMod = 0.1f;



            public const float PoisonStrikeMod = 0.8f;
            public const float DispelStrikeMod = 0.8f;
            public const float ManaBurnStrikeMod = 0.8f;
            public const float ManaBurnStrikeMpDamageMod = 0.8f;


            public const float FlurryOfBlowsMod = 0.75f;




            public const int PotionHealAmount = 500;
            public const int HighPotionHealAmount = 1500;
            public const int EtherMPHealAmount = 100;
            public const int HighEtherMPHealAmount = 200;
            public const int ElixerHealAmount = 1500;
            public const int ElixerMPHealAmount = 200;
            public const int MegaElixerHealAmount = 1500;
            public const int MegaElixerMPHealAmount = 200;
            //public const int MegaElixer = 205;
            //public const int PhoenixDown = 206;
            public const int PhoenixDownHealAmount = 500;

        }
        static public class PerkID
        {
            public const int TestPerk1 = 1;
            public const int TestPerk2 = 2;
            public const int TestPerk3 = 3;

            public const int ImprovedEndurance = 4;

            public const int SpellMastery = 5;

            public const int ImprovedEnduranceLevel2 = 6;



            public const int StatsAllSmall = 100;
            public const int StatsAllSmall2 = 101;
            public const int StatsAllSmall3 = 102;


            public const int StatsAttackSmall = 103;
            public const int StatsAttackMedium = 104;
            public const int StatsAttackLarge = 105;



            public const int StatsDefenseSmall = 106;
            public const int StatsDefenseMedium = 107;
            public const int StatsDefenseLarge = 108;


            public const int StatsMagicAttackSmall = 109;
            public const int StatsMagicAttackMedium = 110;
            public const int StatsMagicAttackLarge = 111;


            public const int StatsMagicDefenseSmall = 112;
            public const int StatsMagicDefenseMedium = 113;
            public const int StatsMagicDefenseLarge = 114;


            public const int StatsSpeedSmall = 115;
            public const int StatsSpeedMedium = 116;
            public const int StatsSpeedLarge = 117;



            public const int StatsHPSmall = 118;
            public const int StatsHPMedium = 119;
            public const int StatsHPLarge = 120;

            public const int StatsMPSmall = 121;
            public const int StatsMPMedium = 122;
            public const int StatsMPLarge = 123;




            public const int FighterCover = 201;
            public const int FighterBrave = 202;
            public const int FighterFaith = 203;
            public const int FighterCure = 204;
            public const int FighterEsuna = 205;



            public const int BlackMageBlackMagic = 301;
            public const int BlackMageBlackMagic2 = 302;
            public const int BlackMageBlackMagic3 = 303;
            public const int BlackMageBlackMagic4 = 304;
            public const int BlackMageArcaneSaboteur = 305;
            public const int BlackMageArcaneSaboteur2 = 306;
            public const int BlackMageArcaneSaboteur3 = 307;



            public const int WhiteMageWhiteMagic = 401;
            public const int WhiteMageWhiteMagic2 = 402;
            public const int WhiteMageWhiteMagic3 = 403;
            public const int WhiteMageWhiteMagic4 = 404;
            public const int WhiteMageEnhancer = 405;
            public const int WhiteMageEnhancer2 = 406;
            public const int WhiteMageEnhancer3 = 407;


            public const int RedMageBlackMagic = 501;
            public const int RedMageBlackMagic2 = 502;
            public const int RedMageBlackMagic3 = 503;

            public const int RedMageWhiteMagic = 504;
            public const int RedMageWhiteMagic2 = 505;
            public const int RedMageWhiteMagic3 = 506;

            public const int RedMageAlchemist = 507;
            public const int RedMageAlchemist2 = 508;
            public const int RedMageAlchemist3 = 509;


            public const int ThiefSteal = 601;
            public const int ThiefPoisonStrike = 602;
            public const int ThiefManaBurnStrike = 603;
            public const int ThiefSilenceStrike = 604;
            public const int ThiefSneakAttack = 605;
            public const int ThiefItemJockey = 606;


            public const int MonkChakra = 701;
            public const int MonkDeBrave = 702;
            public const int MonkDeFaith = 703;
            public const int MonkDeShell = 704;
            public const int MonkDeProtect = 705;
            public const int MonkFlurryOfBlows = 706;
            public const int MonkDispelStrike = 707;



            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(PerkID.TestPerk1, "Test Perk 1");
                lookUp.Add(PerkID.TestPerk2, "Test Perk 2");
                lookUp.Add(PerkID.TestPerk3, "Test Perk 3");
                lookUp.Add(PerkID.ImprovedEndurance, "Improved Endurance");
                lookUp.Add(PerkID.ImprovedEnduranceLevel2, "Improved Endurance Lv2");
                lookUp.Add(PerkID.SpellMastery, "Spell Mastery");

                lookUp.Add(PerkID.StatsAllSmall, "Stats All Small");
                lookUp.Add(PerkID.StatsAllSmall2, "StatsAllSmall2");
                lookUp.Add(PerkID.StatsAllSmall3, "StatsAllSmall3");
                lookUp.Add(PerkID.StatsAttackSmall, "StatsAttackSmall");
                lookUp.Add(PerkID.StatsAttackMedium, "StatsAttackMedium");
                lookUp.Add(PerkID.StatsAttackLarge, "StatsAttackLarge");
                lookUp.Add(PerkID.StatsDefenseSmall, "StatsDefenseSmall");
                lookUp.Add(PerkID.StatsDefenseMedium, "StatsDefenseMedium");
                lookUp.Add(PerkID.StatsDefenseLarge, "StatsDefenseLarge");
                lookUp.Add(PerkID.StatsMagicAttackSmall, "StatsMagicAttackSmall");
                lookUp.Add(PerkID.StatsMagicAttackMedium, "StatsMagicAttackMedium");
                lookUp.Add(PerkID.StatsMagicAttackLarge, "StatsMagicAttackLarge");
                lookUp.Add(PerkID.StatsMagicDefenseSmall, "StatsMagicDefenseSmall");
                lookUp.Add(PerkID.StatsMagicDefenseMedium, "StatsMagicDefenseMedium");
                lookUp.Add(PerkID.StatsMagicDefenseLarge, "StatsMagicDefenseLarge");
                lookUp.Add(PerkID.StatsSpeedSmall, "StatsSpeedSmall");
                lookUp.Add(PerkID.StatsSpeedMedium, "StatsSpeedMedium");
                lookUp.Add(PerkID.StatsSpeedLarge, "StatsSpeedLarge");
                lookUp.Add(PerkID.StatsHPSmall, "StatsHPSmall");
                lookUp.Add(PerkID.StatsHPMedium, "StatsHPMedium");
                lookUp.Add(PerkID.StatsHPLarge, "StatsHPLarge");
                lookUp.Add(PerkID.StatsMPSmall, "StatsMPSmall");
                lookUp.Add(PerkID.StatsMPMedium, "StatsMPMedium");
                lookUp.Add(PerkID.StatsMPLarge, "StatsMPLarge");



                lookUp.Add(PerkID.FighterCover, "Cover");
                lookUp.Add(PerkID.FighterBrave, "Brave");
                lookUp.Add(PerkID.FighterFaith, "Faith");
                lookUp.Add(PerkID.FighterCure, "Cure");
                lookUp.Add(PerkID.FighterEsuna, "Esuna");

                lookUp.Add(PerkID.BlackMageBlackMagic, "Black Magic");
                lookUp.Add(PerkID.BlackMageBlackMagic2, "Black Magic 2");
                lookUp.Add(PerkID.BlackMageBlackMagic3, "Black Magic 3");
                lookUp.Add(PerkID.BlackMageBlackMagic4, "Black Magic 4");
                lookUp.Add(PerkID.BlackMageArcaneSaboteur, "Arcane Saboteur");
                lookUp.Add(PerkID.BlackMageArcaneSaboteur2, "Arcane Saboteur 2");
                lookUp.Add(PerkID.BlackMageArcaneSaboteur3, "Arcane Saboteur 3");
                //lookUp.Add(PerkID.BlackMageArcaneSaboteur4, "Arcane Saboteur 4");


                lookUp.Add(PerkID.WhiteMageWhiteMagic, "White Magic");
                lookUp.Add(PerkID.WhiteMageWhiteMagic2, "White Magic 2");
                lookUp.Add(PerkID.WhiteMageWhiteMagic3, "White Magic 3");
                lookUp.Add(PerkID.WhiteMageWhiteMagic4, "White Magic 4");
                lookUp.Add(PerkID.WhiteMageEnhancer, "Enhancer");
                lookUp.Add(PerkID.WhiteMageEnhancer2, "Enhancer 2");
                lookUp.Add(PerkID.WhiteMageEnhancer3, "Enhancer 3");


                lookUp.Add(PerkID.RedMageBlackMagic, "Black Magic");
                lookUp.Add(PerkID.RedMageBlackMagic2, "Black Magic 2");
                lookUp.Add(PerkID.RedMageBlackMagic3, "Black Magic 3");
                lookUp.Add(PerkID.RedMageWhiteMagic, "White Magic");
                lookUp.Add(PerkID.RedMageWhiteMagic2, "White Magic 2");
                lookUp.Add(PerkID.RedMageWhiteMagic3, "White Magic 3");
                lookUp.Add(PerkID.RedMageAlchemist, "Alchemist");
                lookUp.Add(PerkID.RedMageAlchemist2, "Alchemist 2");
                lookUp.Add(PerkID.RedMageAlchemist3, "Alchemist 3");


                lookUp.Add(PerkID.ThiefSteal, "Steal");
                lookUp.Add(PerkID.ThiefPoisonStrike, "Poison Strike");
                lookUp.Add(PerkID.ThiefManaBurnStrike, "Mana Burn Strike");
                lookUp.Add(PerkID.ThiefSilenceStrike, "Silencing Strike");
                lookUp.Add(PerkID.ThiefSneakAttack, "Sneak Attack");
                lookUp.Add(PerkID.ThiefItemJockey, "Item Jockey");


                lookUp.Add(PerkID.MonkChakra, "Chakra");
                lookUp.Add(PerkID.MonkDeBrave, "DeBrave");
                lookUp.Add(PerkID.MonkDeFaith, "DeFaith");
                lookUp.Add(PerkID.MonkDeShell, "DeShell");
                lookUp.Add(PerkID.MonkDeProtect, "DeProtect");
                lookUp.Add(PerkID.MonkFlurryOfBlows, "Flurry of Blows");
                lookUp.Add(PerkID.MonkDispelStrike, "Dispel Strike");

            }

        }
        static public class PerkTreeID
        {
            public const int TestPerkTree1 = 1;
            public const int TestPerkTree2 = 2;

            public const int FighterTank = 3;
            public const int BlackMageDPS = 4;

            public const int GeneralStats = 100;

            public const int FighterAbilities = 201;
            public const int BlackMageAbilities = 202;
            public const int WhiteMageAbilities = 203;
            public const int RedMageAbilities = 204;
            public const int MonkAbilities = 205;
            public const int TheifAbilities = 206;


            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(PerkTreeID.TestPerkTree1, "Test Perk Tree 1");
                lookUp.Add(PerkTreeID.TestPerkTree2, "Test Perk Tree 2");
                lookUp.Add(PerkTreeID.FighterTank, "Tank");
                lookUp.Add(PerkTreeID.BlackMageDPS, "Ravager");

                lookUp.Add(PerkTreeID.GeneralStats, "General Stats");

                lookUp.Add(PerkTreeID.FighterAbilities, "Fighter");
                lookUp.Add(PerkTreeID.BlackMageAbilities, "Black Mage");
                lookUp.Add(PerkTreeID.WhiteMageAbilities, "White Mage");
                lookUp.Add(PerkTreeID.RedMageAbilities, "Red Mage");
                lookUp.Add(PerkTreeID.MonkAbilities, "Monk");
                lookUp.Add(PerkTreeID.TheifAbilities, "Theif");

            }

        }
        public static partial class ContentLoader
        {
            static public int LookUpItemCost(int itemID)
            {
                if (itemID == ItemID.Potion)
                    return 10;
                else if (itemID == ItemID.HighPotion)
                    return 30;
                else if (itemID == ItemID.Ether)
                    return 20;
                else if (itemID == ItemID.HighEther)
                    return 40;
                else if (itemID == ItemID.Elixer)
                    return 60;
                else if (itemID == ItemID.MegaElixer)
                    return 100;
                else if (itemID == ItemID.PhoenixDown)
                    return 30;
                else if (itemID == ItemID.EchoHerbs)
                    return 10;
                else if (itemID == ItemID.Antidote)
                    return 10;
                else if (itemID == ItemID.GysahlGreens)
                    return 10;
                else if (itemID == ItemID.GoldenNeedle)
                    return 10;
                else if (itemID == ItemID.Remedy)
                    return 30;

                return 99;

            }
            static public int GetMPCostForBattleAction(int battleActionID)
            {

                if (battleActionID == BattleActionID.Blizzard || battleActionID == BattleActionID.Fire || battleActionID == BattleActionID.Thunder || battleActionID == BattleActionID.Cure)
                    return 10;
                else if (battleActionID == BattleActionID.Blizzara || battleActionID == BattleActionID.Fira || battleActionID == BattleActionID.Thundara || battleActionID == BattleActionID.Cura)
                    return 20;
                else if (battleActionID == BattleActionID.Blizzaga || battleActionID == BattleActionID.Firaga || battleActionID == BattleActionID.Thundaga || battleActionID == BattleActionID.Curaga)
                    return 40;
                else if (battleActionID == BattleActionID.BlizzardAll || battleActionID == BattleActionID.FireAll || battleActionID == BattleActionID.ThunderAll)
                    return 15;
                else if (battleActionID == BattleActionID.BlizzaraAll || battleActionID == BattleActionID.FiraAll || battleActionID == BattleActionID.ThundaraAll)
                    return 30;
                else if (battleActionID == BattleActionID.BlizzagaAll || battleActionID == BattleActionID.FiragaAll || battleActionID == BattleActionID.ThundagaAll)
                    return 55;

                else if (battleActionID == BattleActionID.Life)
                    return 15;
                else if (battleActionID == BattleActionID.Lifa)
                    return 30;
                else if (battleActionID == BattleActionID.Lifaga)
                    return 55;
                else if (battleActionID == BattleActionID.Meteor)
                    return 100;
                else if (battleActionID == BattleActionID.Flare)
                    return 60;

                else if (battleActionID == BattleActionID.Haste || battleActionID == BattleActionID.Slow)
                    return 10;
                else if (battleActionID == BattleActionID.Regen)
                    return 10;

                else if (battleActionID == BattleActionID.Protect || battleActionID == BattleActionID.Shell || battleActionID == BattleActionID.Faith || battleActionID == BattleActionID.Brave)
                    return 10;

                else if (battleActionID == BattleActionID.Debrave || battleActionID == BattleActionID.Defaith || battleActionID == BattleActionID.Deprotect || battleActionID == BattleActionID.Deshell)
                    return 10;

                else if (battleActionID == BattleActionID.Silence)
                    return 15;
                else if (battleActionID == BattleActionID.Beserk)
                    return 15;
                else if (battleActionID == BattleActionID.Petrify)
                    return 15;
                else if (battleActionID == BattleActionID.Bubble)
                    return 25;
                else if (battleActionID == BattleActionID.AutoLife)
                    return 35;
                else if (battleActionID == BattleActionID.Doom)
                    return 25;

                else if (battleActionID == BattleActionID.Esuna)
                    return 30;
                else if (battleActionID == BattleActionID.Dispel)
                    return 25;
                else if (battleActionID == BattleActionID.PoisonStrike)
                    return 10;
                else if (battleActionID == BattleActionID.DispelStrike)
                    return 25;
                else if (battleActionID == BattleActionID.ManaBurnStrike)
                    return 15;
                else if (battleActionID == BattleActionID.FlurryOfBlows)
                    return 20;

                else if (battleActionID == BattleActionID.CraftPotion)
                    return 10;
                else if (battleActionID == BattleActionID.CraftHighPotion)
                    return 20;
                else if (battleActionID == BattleActionID.CraftEther)
                    return 20;
                else if (battleActionID == BattleActionID.CraftHighEther)
                    return 30;
                else if (battleActionID == BattleActionID.CraftElixer)
                    return 40;
                else if (battleActionID == BattleActionID.CraftMegaElixer)
                    return 80;
                else if (battleActionID == BattleActionID.CraftPhoenixDown)
                    return 20;
                else if (battleActionID == BattleActionID.CraftRemedy)
                    return 20;
                else if (battleActionID == BattleActionID.CraftEchoHerbs || battleActionID == BattleActionID.CraftAntidote || battleActionID == BattleActionID.CraftGysahlGreens || battleActionID == BattleActionID.CraftGoldenNeedle)
                    return 10;

                // else if (battleActionID == BattleActionID.)
                //     return;
                // else if (battleActionID == BattleActionID.)
                //     return;

                // else if (battleActionID == BattleActionID. || battleActionID == BattleActionID. )
                // return;

                // else if (battleActionID == BattleActionID. || battleActionID == BattleActionID. || battleActionID == BattleActionID. || battleActionID == BattleActionID.)
                // return ;

                return 0;
            }

        }
        public static class PerkContent
        {

            static public LinkedList<int> GetPerkTreesForClass(int classID)
            {

                LinkedList<int> techTrees = new LinkedList<int>();

                if (classID == CharacterClassID.Fighter)
                {
                    // techTrees.AddLast(PerkTreeID.TestPerkTree1);
                    // techTrees.AddLast(PerkTreeID.TestPerkTree2);
                    techTrees.AddLast(PerkTreeID.GeneralStats);
                    techTrees.AddLast(PerkTreeID.FighterAbilities);
                }
                else if (classID == CharacterClassID.BlackMage)
                {
                    techTrees.AddLast(PerkTreeID.GeneralStats);
                    //techTrees.AddLast(PerkTreeID.TestPerkTree2);
                    techTrees.AddLast(PerkTreeID.BlackMageAbilities);
                }
                else if (classID == CharacterClassID.Monk)
                {
                    techTrees.AddLast(PerkTreeID.GeneralStats);
                    techTrees.AddLast(PerkTreeID.MonkAbilities);
                }
                else if (classID == CharacterClassID.WhiteMage)
                {
                    techTrees.AddLast(PerkTreeID.GeneralStats);
                    techTrees.AddLast(PerkTreeID.WhiteMageAbilities);
                }
                else if (classID == CharacterClassID.RedMage)
                {
                    techTrees.AddLast(PerkTreeID.GeneralStats);
                    techTrees.AddLast(PerkTreeID.RedMageAbilities);
                }
                else if (classID == CharacterClassID.Thief)
                {
                    techTrees.AddLast(PerkTreeID.GeneralStats);
                    techTrees.AddLast(PerkTreeID.TheifAbilities);
                }
                else
                {
                    techTrees.AddLast(PerkTreeID.TestPerkTree1);
                }

                return techTrees;

            }
            static public LinkedList<int> GetPerksForPerkTree(int perkTreeID)
            {

                LinkedList<int> perkTree = new LinkedList<int>();

                if (perkTreeID == PerkTreeID.TestPerkTree1)
                {
                    perkTree.AddLast(PerkID.TestPerk1);
                    perkTree.AddLast(PerkID.TestPerk2);
                    perkTree.AddLast(PerkID.TestPerk3);
                }
                else if (perkTreeID == PerkTreeID.TestPerkTree2)
                {
                    perkTree.AddLast(PerkID.TestPerk3);
                    perkTree.AddLast(PerkID.TestPerk3);
                    perkTree.AddLast(PerkID.TestPerk3);
                }
                else if (perkTreeID == PerkTreeID.BlackMageDPS)
                {
                    perkTree.AddLast(PerkID.SpellMastery);
                    perkTree.AddLast(PerkID.SpellMastery);
                    perkTree.AddLast(PerkID.SpellMastery);
                    perkTree.AddLast(PerkID.SpellMastery);
                }
                else if (perkTreeID == PerkTreeID.FighterTank)
                {
                    perkTree.AddLast(PerkID.ImprovedEndurance);
                    perkTree.AddLast(PerkID.ImprovedEnduranceLevel2);
                    perkTree.AddLast(PerkID.ImprovedEndurance);
                    perkTree.AddLast(PerkID.ImprovedEndurance);
                }


                else if (perkTreeID == PerkTreeID.GeneralStats)
                {
                    perkTree.AddLast(PerkID.StatsAllSmall);
                    perkTree.AddLast(PerkID.StatsAllSmall2);
                    perkTree.AddLast(PerkID.StatsAllSmall3);

                    perkTree.AddLast(PerkID.StatsAttackSmall);
                    perkTree.AddLast(PerkID.StatsAttackMedium);
                    perkTree.AddLast(PerkID.StatsDefenseSmall);
                    perkTree.AddLast(PerkID.StatsDefenseMedium);
                    perkTree.AddLast(PerkID.StatsMagicAttackSmall);
                    perkTree.AddLast(PerkID.StatsMagicAttackMedium);
                    perkTree.AddLast(PerkID.StatsMagicDefenseSmall);
                    perkTree.AddLast(PerkID.StatsMagicDefenseMedium);
                    perkTree.AddLast(PerkID.StatsSpeedSmall);
                    perkTree.AddLast(PerkID.StatsSpeedMedium);
                    perkTree.AddLast(PerkID.StatsHPSmall);
                    perkTree.AddLast(PerkID.StatsHPMedium);
                    perkTree.AddLast(PerkID.StatsMPSmall);
                    perkTree.AddLast(PerkID.StatsMPMedium);

                }


                else if (perkTreeID == PerkTreeID.FighterAbilities)
                {
                    perkTree.AddLast(PerkID.FighterCover);
                    perkTree.AddLast(PerkID.FighterBrave);
                    perkTree.AddLast(PerkID.FighterFaith);
                    perkTree.AddLast(PerkID.FighterCure);
                    perkTree.AddLast(PerkID.FighterEsuna);

                }

                else if (perkTreeID == PerkTreeID.BlackMageAbilities)
                {
                    perkTree.AddLast(PerkID.BlackMageBlackMagic);
                    perkTree.AddLast(PerkID.BlackMageBlackMagic2);
                    perkTree.AddLast(PerkID.BlackMageBlackMagic3);
                    perkTree.AddLast(PerkID.BlackMageBlackMagic4);
                    perkTree.AddLast(PerkID.BlackMageArcaneSaboteur);
                    perkTree.AddLast(PerkID.BlackMageArcaneSaboteur2);
                    perkTree.AddLast(PerkID.BlackMageArcaneSaboteur3);
                    //perkTree.AddLast(PerkID.BlackMageArcaneSaboteur4);
                }

                else if (perkTreeID == PerkTreeID.WhiteMageAbilities)
                {
                    perkTree.AddLast(PerkID.WhiteMageWhiteMagic);
                    perkTree.AddLast(PerkID.WhiteMageWhiteMagic2);
                    perkTree.AddLast(PerkID.WhiteMageWhiteMagic3);
                    perkTree.AddLast(PerkID.WhiteMageWhiteMagic4);
                    perkTree.AddLast(PerkID.WhiteMageEnhancer);
                    perkTree.AddLast(PerkID.WhiteMageEnhancer2);
                    perkTree.AddLast(PerkID.WhiteMageEnhancer3);
                }

                else if (perkTreeID == PerkTreeID.RedMageAbilities)
                {
                    perkTree.AddLast(PerkID.RedMageBlackMagic);
                    perkTree.AddLast(PerkID.RedMageBlackMagic2);
                    perkTree.AddLast(PerkID.RedMageBlackMagic3);
                    perkTree.AddLast(PerkID.RedMageWhiteMagic);
                    perkTree.AddLast(PerkID.RedMageWhiteMagic2);
                    perkTree.AddLast(PerkID.RedMageWhiteMagic3);
                    perkTree.AddLast(PerkID.RedMageAlchemist);
                    perkTree.AddLast(PerkID.RedMageAlchemist2);
                    perkTree.AddLast(PerkID.RedMageAlchemist3);
                }

                else if (perkTreeID == PerkTreeID.TheifAbilities)
                {
                    perkTree.AddLast(PerkID.ThiefSteal);
                    perkTree.AddLast(PerkID.ThiefPoisonStrike);
                    perkTree.AddLast(PerkID.ThiefManaBurnStrike);
                    //perkTree.AddLast(PerkID.ThiefSilenceStrike);
                    perkTree.AddLast(PerkID.ThiefSneakAttack);
                    perkTree.AddLast(PerkID.ThiefItemJockey);
                }

                else if (perkTreeID == PerkTreeID.MonkAbilities)
                {
                    perkTree.AddLast(PerkID.MonkChakra);
                    perkTree.AddLast(PerkID.MonkDeBrave);
                    perkTree.AddLast(PerkID.MonkDeFaith);
                    perkTree.AddLast(PerkID.MonkDeShell);
                    perkTree.AddLast(PerkID.MonkDeProtect);
                    perkTree.AddLast(PerkID.MonkFlurryOfBlows);
                    perkTree.AddLast(PerkID.MonkDispelStrike);
                }

                return perkTree;

            }
            static public string GetPerkInfo(int perkID)
            {

                if (perkID == PerkID.SpellMastery)
                    return "Increases Magic Attack +X";
                else if (perkID == PerkID.ImprovedEndurance)
                    return "Increases Max HP +X";


                else if (perkID == PerkID.StatsAllSmall || perkID == PerkID.StatsAllSmall2 || perkID == PerkID.StatsAllSmall3)
                    return "Increases all stats by a small amount";

                else if (perkID == PerkID.StatsAttackSmall)
                    return "Increases Attack by " + StatBuff.Attack_Low;
                else if (perkID == PerkID.StatsAttackMedium)
                    return "Increases Attack by " + StatBuff.Attack_Medium;
                else if (perkID == PerkID.StatsAttackLarge)
                    return "Increases Attack by " + StatBuff.Attack_High;

                else if (perkID == PerkID.StatsDefenseSmall)
                    return "Increases Defense by " + StatBuff.Defense_Low;
                else if (perkID == PerkID.StatsDefenseMedium)
                    return "Increases Defense by " + StatBuff.Defense_Medium;
                else if (perkID == PerkID.StatsDefenseLarge)
                    return "Increases Defense by " + StatBuff.Defense_High;

                else if (perkID == PerkID.StatsMagicAttackSmall)
                    return "Increases Magic Attack by " + StatBuff.MagicAttack_Low;
                else if (perkID == PerkID.StatsMagicAttackMedium)
                    return "Increases Magic Attack by " + StatBuff.MagicAttack_Medium;
                else if (perkID == PerkID.StatsMagicAttackLarge)
                    return "Increases Magic Attack by " + StatBuff.MagicAttack_High;

                else if (perkID == PerkID.StatsMagicDefenseSmall)
                    return "Increases Magic Defense by " + StatBuff.MagicDefense_Low;
                else if (perkID == PerkID.StatsMagicDefenseMedium)
                    return "Increases Magic Defense by " + StatBuff.MagicDefense_Medium;
                else if (perkID == PerkID.StatsMagicDefenseLarge)
                    return "Increases Magic Defense by " + StatBuff.MagicDefense_High;

                else if (perkID == PerkID.StatsSpeedSmall)
                    return "Increases Speed by " + StatBuff.Speed_Low;
                else if (perkID == PerkID.StatsSpeedMedium)
                    return "Increases Speed by " + StatBuff.Speed_Medium;
                else if (perkID == PerkID.StatsSpeedLarge)
                    return "Increases Speed by " + StatBuff.Speed_High;

                else if (perkID == PerkID.StatsHPSmall)
                    return "Increases Max HP by " + StatBuff.HP_Low;
                else if (perkID == PerkID.StatsHPMedium)
                    return "Increases Max HP by " + StatBuff.HP_Medium;
                else if (perkID == PerkID.StatsHPLarge)
                    return "Increases Max HP by " + StatBuff.HP_High;

                else if (perkID == PerkID.StatsMPSmall)
                    return "Increases Max MP by " + StatBuff.MP_Low;
                else if (perkID == PerkID.StatsMPMedium)
                    return "Increases Max MP by " + StatBuff.MP_Medium;
                else if (perkID == PerkID.StatsMPLarge)
                    return "Increases Max MP by " + StatBuff.MP_High;


                return ".....";
            }
            static public int LookUpPerkCost(int perkID)
            {
                if (perkID == PerkID.TestPerk1)
                    return 10;
                else if (perkID == PerkID.TestPerk2)
                    return 20;
                else if (perkID == PerkID.TestPerk3)
                    return 30;


                else if (perkID == PerkID.StatsAllSmall
                || perkID == PerkID.StatsAllSmall2
                || perkID == PerkID.StatsAllSmall3)
                    return PerkGilCosts.NormalPerk;

                else if (perkID == PerkID.StatsAttackSmall
                || perkID == PerkID.StatsDefenseSmall
                || perkID == PerkID.StatsMagicAttackSmall
                || perkID == PerkID.StatsMagicDefenseSmall
                || perkID == PerkID.StatsSpeedSmall
                || perkID == PerkID.StatsHPSmall
                || perkID == PerkID.StatsMPSmall)
                    return PerkGilCosts.SmallPerk;

                else if (perkID == PerkID.StatsAttackMedium
                || perkID == PerkID.StatsDefenseMedium
                || perkID == PerkID.StatsMagicAttackMedium
                || perkID == PerkID.StatsMagicDefenseMedium
                || perkID == PerkID.StatsSpeedMedium
                || perkID == PerkID.StatsHPMedium
                || perkID == PerkID.StatsMPMedium)
                    return PerkGilCosts.NormalPerk;




                return PerkGilCosts.NormalPerk;

            }

            static public LinkedList<int> GetPerkRequirements(int perkID)
            {
                LinkedList<int> requirements = new LinkedList<int>();

                if (perkID == PerkID.ImprovedEnduranceLevel2)
                {
                    requirements.AddLast(PerkID.ImprovedEndurance);
                }


                else if (perkID == PerkID.StatsAllSmall2)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsAllSmall3)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }

                else if (perkID == PerkID.StatsAttackSmall)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsAttackMedium)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }

                else if (perkID == PerkID.StatsDefenseSmall)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsDefenseMedium)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }

                else if (perkID == PerkID.StatsMagicAttackSmall)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsMagicAttackMedium)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }


                else if (perkID == PerkID.StatsMagicDefenseSmall)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsMagicDefenseMedium)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }

                else if (perkID == PerkID.StatsSpeedSmall)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsSpeedMedium)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }

                else if (perkID == PerkID.StatsHPSmall)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsHPMedium)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }

                else if (perkID == PerkID.StatsMPSmall)
                {
                    requirements.AddLast(PerkID.StatsAllSmall);
                }
                else if (perkID == PerkID.StatsMPMedium)
                {
                    requirements.AddLast(PerkID.StatsAllSmall2);
                }



                else if (perkID == PerkID.FighterCure)
                {
                    requirements.AddLast(PerkID.FighterBrave);
                    requirements.AddLast(PerkID.FighterFaith);
                }
                else if (perkID == PerkID.FighterEsuna)
                {
                    requirements.AddLast(PerkID.FighterBrave);
                    requirements.AddLast(PerkID.FighterFaith);
                }

                else if (perkID == PerkID.BlackMageBlackMagic2)
                {
                    requirements.AddLast(PerkID.BlackMageBlackMagic);
                }
                else if (perkID == PerkID.BlackMageBlackMagic3)
                {
                    requirements.AddLast(PerkID.BlackMageBlackMagic2);
                }
                else if (perkID == PerkID.BlackMageBlackMagic4)
                {
                    requirements.AddLast(PerkID.BlackMageBlackMagic3);
                }

                else if (perkID == PerkID.BlackMageArcaneSaboteur2)
                {
                    requirements.AddLast(PerkID.BlackMageArcaneSaboteur);
                }
                else if (perkID == PerkID.BlackMageArcaneSaboteur3)
                {
                    requirements.AddLast(PerkID.BlackMageArcaneSaboteur2);
                }
                // else if (perkID == PerkID.BlackMageArcaneSaboteur4)
                // {
                //     requirements.AddLast(PerkID.BlackMageArcaneSaboteur3);
                // }

                else if (perkID == PerkID.WhiteMageWhiteMagic2)
                {
                    requirements.AddLast(PerkID.WhiteMageWhiteMagic);
                }
                else if (perkID == PerkID.WhiteMageWhiteMagic3)
                {
                    requirements.AddLast(PerkID.WhiteMageWhiteMagic2);
                }
                else if (perkID == PerkID.WhiteMageWhiteMagic4)
                {
                    requirements.AddLast(PerkID.WhiteMageWhiteMagic3);
                }
                else if (perkID == PerkID.WhiteMageEnhancer2)
                {
                    requirements.AddLast(PerkID.WhiteMageEnhancer);
                }
                else if (perkID == PerkID.WhiteMageEnhancer3)
                {
                    requirements.AddLast(PerkID.WhiteMageEnhancer2);
                }


                else if (perkID == PerkID.RedMageBlackMagic2)
                {
                    requirements.AddLast(PerkID.RedMageBlackMagic);
                }
                else if (perkID == PerkID.RedMageBlackMagic3)
                {
                    requirements.AddLast(PerkID.RedMageBlackMagic2);
                }

                else if (perkID == PerkID.RedMageWhiteMagic2)
                {
                    requirements.AddLast(PerkID.RedMageWhiteMagic);
                }
                else if (perkID == PerkID.RedMageWhiteMagic3)
                {
                    requirements.AddLast(PerkID.RedMageWhiteMagic2);
                }

                else if (perkID == PerkID.RedMageAlchemist2)
                {
                    requirements.AddLast(PerkID.RedMageAlchemist);
                }
                else if (perkID == PerkID.RedMageAlchemist3)
                {
                    requirements.AddLast(PerkID.RedMageAlchemist2);
                }

                else if (perkID == PerkID.ThiefManaBurnStrike)
                {
                    requirements.AddLast(PerkID.ThiefPoisonStrike);
                }
                else if (perkID == PerkID.ThiefSilenceStrike)
                {
                    requirements.AddLast(PerkID.ThiefManaBurnStrike);
                }
                else if (perkID == PerkID.ThiefSneakAttack)
                {
                    requirements.AddLast(PerkID.ThiefSteal);
                }
                else if (perkID == PerkID.ThiefItemJockey)
                {
                    requirements.AddLast(PerkID.ThiefSteal);
                }

                else if (perkID == PerkID.MonkDispelStrike)
                {
                    requirements.AddLast(PerkID.MonkDeBrave);
                    requirements.AddLast(PerkID.MonkDeFaith);
                    requirements.AddLast(PerkID.MonkDeShell);
                    requirements.AddLast(PerkID.MonkDeProtect);
                }


                return requirements;
            }

        }
        public static class PerkGilCosts
        {
            public const int TinyPerk = 5;
            public const int SmallPerk = 10;
            public const int NormalPerk = 15;
            public const int BigPerk = 20;
            public const int HugePerk = 25;

        }
        public static class BattleActionProcessing
        {

            static private int CalculateDefenseDamageReduction(int attack, float attackModifier, int defense)
            {
                float baseDamageAmount = (float)attack * attackModifier;
                float reductionAmount = 1f - (float)defense / 1000f;
                int damageAmount = (int)(baseDamageAmount * reductionAmount);

                if (damageAmount < 0)
                    damageAmount = 0;

                return damageAmount;
            }
            static private int CalculateHealingAmountModification(int magicAttack, float modifier)
            {
                int amount = (int)(((float)magicAttack) * modifier);

                if (amount < 0)
                    amount = 0;

                return amount;
            }

        }
        public static class BaseStats
        {

            public const int HP_High = 1000;
            public const int HP_Medium = 750;
            public const int HP_Low = 500;

            public const int MP_High = 100;
            public const int MP_Medium = 75;
            public const int MP_Low = 50;

            public const int Attack_High = 250;
            public const int Attack_Medium = 190;
            public const int Attack_Low = 130;

            public const int MagicAttack_High = 250;
            public const int MagicAttack_Medium = 190;
            public const int MagicAttack_Low = 130;

            public const int Defense_High = 20;
            public const int Defense_Medium = 10;
            public const int Defense_Low = 5;


            public const int MagicDefense_High = 20;
            public const int MagicDefense_Medium = 10;
            public const int MagicDefense_Low = 5;

            public const int Speed_High = 80;
            public const int Speed_Medium = 60;
            public const int Speed_Low = 40;

        }
        public static class StatBuff
        {

            public const int HP_Huge = 250;
            public const int HP_High = 200;
            public const int HP_Medium = 150;
            public const int HP_Low = 100;
            public const int HP_Tiny = 50;


            public const int MP_Huge = 25;
            public const int MP_High = 20;
            public const int MP_Medium = 15;
            public const int MP_Low = 10;
            public const int MP_Tiny = 5;

            public const int Attack_Huge = 100;
            public const int Attack_High = 80;
            public const int Attack_Medium = 60;
            public const int Attack_Low = 40;
            public const int Attack_Tiny = 20;

            public const int MagicAttack_Huge = 100;
            public const int MagicAttack_High = 80;
            public const int MagicAttack_Medium = 60;
            public const int MagicAttack_Low = 40;
            public const int MagicAttack_Tiny = 20;

            public const int Defense_Huge = 12;
            public const int Defense_High = 10;
            public const int Defense_Medium = 8;
            public const int Defense_Low = 6;
            public const int Defense_Tiny = 4;


            public const int MagicDefense_Huge = 12;
            public const int MagicDefense_High = 10;
            public const int MagicDefense_Medium = 8;
            public const int MagicDefense_Low = 6;
            public const int MagicDefense_Tiny = 4;


            public const int Speed_Huge = 25;
            public const int Speed_High = 20;
            public const int Speed_Medium = 15;
            public const int Speed_Low = 10;
            public const int Speed_Tiny = 5;

        }
        public static class Gil
        {
            public const int TotalGil = 1000;
            public const int GilCostForPartyMember = 100;
        }

    }
}
