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
using RimWorld;
using UnityEngine;
using Verse;

namespace RenamePawns
{
    public class Dialog_RenamePawn : Window
    {
        private readonly Pawn pawn;

        private string title;

        private string firstName;

        private string nickName;

        private string lastName;

        public override Vector2 InitialSize => new Vector2(300f, 175f);

        public Dialog_RenamePawn(Pawn pawn)
        {
            this.pawn = pawn;
            if (pawn.Name is NameSingle nameSingle)
            {
                firstName = nameSingle.Name;
            }
            else if (pawn.Name is NameTriple nameTriple)
            {
                firstName = nameTriple.First;
                nickName = nameTriple.Nick.Equals(firstName) ? "" : nameTriple.Nick;
                lastName = nameTriple.Last;
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (pawn.story != null)
            {
                title = pawn.story.Title.Equals(pawn.story.TitleDefault) ? "" : pawn.story.Title;
            }
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            closeOnAccept = false;
        }

        private bool IsNameTripple()
        {
            return nickName != null;
        }

        protected override void SetInitialSizeAndPosition()
        {
            Vector2 initialSize = InitialSize;
            initialSize.x *= IsNameTripple() ? 2 : 1;
            windowRect = new Rect((UI.screenWidth - initialSize.x) / 2f, (UI.screenHeight - initialSize.y) / 2f, initialSize.x, initialSize.y);
            windowRect = GenUI.Rounded(windowRect);
        }

        public override void DoWindowContents(Rect inRect)
        {
            const float height = 35f;
            const float mergin = 15f;
            const float padding = 20;

            float textFieldCount = IsNameTripple() ? 3f : 2f;
            textFieldCount += (title != null) ? 1f : 0f;
            float width = (inRect.width / textFieldCount) - padding;

            float drawCursorX = mergin;
            float drawCursorY = mergin;

            bool okFlag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                okFlag = true;
                Event.current.Use();
            }

            string nameText;
            if (IsNameTripple())
            {
                nameText = string.Format("{0} '{1}' {2}", firstName, nickName.Equals(firstName) ? "" : nickName, lastName).Replace(" '' ", " ");
            }
            else
            {
                nameText = firstName;
            }

            if (title == "")
            {
                nameText = nameText + ", " + pawn.story.TitleDefaultCap;
            }
            else if (title != null)
            {
                nameText = nameText + ", " + GenText.CapitalizeFirst(title);
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(drawCursorX, drawCursorY, inRect.width - mergin, height), nameText);

            drawCursorY += height;

            string firstTextField = Widgets.TextField(new Rect(drawCursorX, drawCursorY, width * (!IsNameTripple() ? 2 : 1), height), firstName);
            if (CharacterCardUtility.ValidNameRegex.IsMatch(firstTextField))
            {
                firstName = firstTextField;
            }
            if (IsNameTripple())
            {
                drawCursorX += width + padding;
                string nickTextField = Widgets.TextField(new Rect(drawCursorX, drawCursorY, width, height), nickName);
                if (CharacterCardUtility.ValidNameRegex.IsMatch(nickTextField))
                {
                    nickName = nickTextField;
                }
                drawCursorX += width + padding;
                string lastTextField = Widgets.TextField(new Rect(drawCursorX, drawCursorY, width, height), lastName);
                if (CharacterCardUtility.ValidNameRegex.IsMatch(lastTextField))
                {
                    lastName = lastTextField;
                }
                drawCursorX += width + padding;
            }
            if (title != null)
            {
                string titleTextField = Widgets.TextField(new Rect(drawCursorX, drawCursorY, width, height), title);
                if (CharacterCardUtility.ValidNameRegex.IsMatch(titleTextField))
                {
                    title = titleTextField;
                }
            }

            drawCursorY += height + mergin;
            drawCursorX = IsNameTripple() ? (drawCursorX - (width + padding)) : mergin;

            if (Widgets.ButtonText(new Rect(drawCursorX, drawCursorY, width, height), "Randomize".Translate()))
            {
                var newName = PawnBioAndNameGenerator.GeneratePawnName(pawn);
                if (newName is NameTriple nameTriple)
                {
                    firstName = nameTriple.First;
                    nickName = nameTriple.Nick.Equals(firstName) ? "" : nameTriple.Nick;
                    lastName = nameTriple.Last;
                }
                else if (newName is NameSingle nameSingle)
                {
                    firstName = nameSingle.Name;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            drawCursorX += width + padding;
            if (Widgets.ButtonText(new Rect(drawCursorX, drawCursorY, width, height), "OK".Translate()) || okFlag)
            {
                if (firstName.NullOrEmpty())
                {
                    firstName = IsNameTripple() ? ((NameTriple)pawn.Name).First : ((NameSingle)pawn.Name).Name;
                }
                if (IsNameTripple() && nickName.NullOrEmpty())
                {
                    nickName = firstName;
                }

                if (IsNameTripple())
                {
                    pawn.Name = new NameTriple(firstName, nickName, lastName);
                }
                else
                {
                    pawn.Name = new NameSingle(firstName);
                }

                if (pawn.story != null)
                {
                    pawn.story.Title = title;
                }

                Find.WindowStack.TryRemove(this);
                Messages.Message(pawn.def.race.Animal ?
                    "AnimalGainsName".Translate(pawn.Name.ToStringFull) :
                    "PawnGainsName".Translate(pawn.Name.ToStringFull, pawn.story.Title, pawn.Named("PAWN")).AdjustedFor(pawn), pawn, MessageTypeDefOf.PositiveEvent, historical: false);
            }
        }
    }
}
