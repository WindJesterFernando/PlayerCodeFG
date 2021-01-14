using System;
using System.IO;
using System.Collections.Generic;

namespace Final_Gambit_Player_Code
{
    static public class PartyAI
    {
        static public void ProcessBlackMageAI()
        {

        }

        static public void ProcessAI()
        {

            PartyCharacter target = null;
            
            foreach(PartyCharacter pc in FinalGambit.BattleState.foeCharacters)//partyCharacters)//FinalGambit.BattleState.foeCharacters)
            {
                if(target == null)
                    target = pc;
                else if(pc.hp > target.hp)
                    target = pc;
            }

            //Console.WriteLine(FinalGambit.BattleState.foeCharacters.Count);

            FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.Attack, target);//FinalGambit.BattleState.foeCharacters.Last.Value);



            //FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.PhoenixDown, target);
            //FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.MegaElixer, target);

        }
    }
}


