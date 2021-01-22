using System;
using System.IO;
using System.Collections.Generic;

namespace Final_Gambit_Player_Code
{
    static public class PartyAI
    {

        static public void ProcessAI()
        {

            //FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.Attack, FinalGambit.BattleState.foeCharacters.Last.Value);



            foreach (RecordOfBattleAction rba in FinalGambit.recordOfBattleActions)
                Console.WriteLine(rba.battleActionID + " - " + rba.characterWithInitiative.classID);


            PartyCharacter target = null;

            foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
            {
                if (target == null)
                    target = pc;
                else if (pc.hp > target.hp)
                    target = pc;
            }

            FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.Attack, target);

        }
    }
}


