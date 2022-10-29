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
    public static class Patches
    {
        static Patches()
        {
            var harmony = new Harmony("rimworld.relta.renamepawns");

            harmony.Patch(
                AccessTools.Method(
                    typeof(PawnNamingUtility),
                    nameof(PawnNamingUtility.NamePawnDialog)
                ),
                postfix: new HarmonyMethod(
                    typeof(Patches),
                    nameof(PawnNamingUtility_NamePawnDialog_Postfix)
                )
            );
        }

        public static void PawnNamingUtility_NamePawnDialog_Postfix(
            ref Dialog_NamePawn __result, Pawn pawn, string initialFirstNameOverride)
        {
            Dictionary<NameFilter, List<string>> suggestedNames = null;
            NameFilter editableNames;
            NameFilter visibleNames;

            if (pawn.babyNamingDeadline >= Find.TickManager.TicksGame || DebugSettings.ShowDevGizmos)
            {
                editableNames = NameFilter.First | NameFilter.Nick | NameFilter.Last;
                visibleNames = NameFilter.First | NameFilter.Nick | NameFilter.Last;
                List<string> list = new List<string>();
                Pawn mother;
                if ((mother = pawn.GetMother()) != null)
                {
                    list.Add(PawnNamingUtility.GetLastName(mother));
                }
                Pawn father;
                if ((father = pawn.GetFather()) != null)
                {
                    list.Add(PawnNamingUtility.GetLastName(father));
                }
                Pawn birthParent;
                if ((birthParent = pawn.GetBirthParent()) != null)
                {
                    list.Add(PawnNamingUtility.GetLastName(birthParent));
                }
                list.RemoveDuplicates();
                suggestedNames = new Dictionary<NameFilter, List<string>> {
                {
                    NameFilter.Last,
                    list
                } };
            }
            else
            {
                visibleNames = NameFilter.First | NameFilter.Nick | NameFilter.Last | NameFilter.Title;
                editableNames = NameFilter.First | NameFilter.Nick | NameFilter.Last | NameFilter.Title;
            }

            __result = new Dialog_NamePawn(
                pawn,
                visibleNames,
                editableNames,
                suggestedNames,
                initialFirstNameOverride
            );
        }
    }
}
