/*
 * Copyright (c) 2021 Nattakit Hosapsin.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RenamePawns
{
    [StaticConstructorOnStartup]
    internal static class Patches
    {
        static Patches()
        {
            var harmony = new Harmony("rimworld.relta.renamepawns");
            harmony.Patch(
                AccessTools.Method(typeof(TrainingCardUtility), "DrawRenameButton"),
                transpiler: new HarmonyMethod(typeof(Patches), "Dialog_NamePawn_CTOR_Transpiler")
            );
            harmony.Patch(
                AccessTools.Method(typeof(CharacterCardUtility), "DrawCharacterCard"),
                transpiler: new HarmonyMethod(typeof(Patches), "Dialog_NamePawn_CTOR_Transpiler")
            );
        }

        public static IEnumerable<CodeInstruction> Dialog_NamePawn_CTOR_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ConstructorInfo namePawnCtor = AccessTools.Constructor(typeof(Dialog_NamePawn), new Type[] { typeof(Pawn) });
            ConstructorInfo renamePawnCtor = AccessTools.Constructor(typeof(Dialog_RenamePawn), new Type[] { typeof(Pawn) });
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj && instruction.OperandIs(namePawnCtor))
                {
                    yield return new CodeInstruction(OpCodes.Newobj, renamePawnCtor);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
