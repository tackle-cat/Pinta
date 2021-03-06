﻿// 
// PasteIntoNewLayerAction.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
// 
// Copyright (c) 2010 Jonathan Pobst
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Gtk;
using Mono.Unix;
using Pinta.Core;

namespace Pinta.Actions
{
	class PasteIntoNewLayerAction : IActionHandler
	{
		#region IActionHandler Members
		public void Initialize ()
		{
			PintaCore.Actions.Edit.PasteIntoNewLayer.Activated += Activated;
		}

		public void Uninitialize ()
		{
			PintaCore.Actions.Edit.PasteIntoNewLayer.Activated -= Activated;
		}
		#endregion

		private void Activated (object sender, EventArgs e)
		{
			Gtk.Clipboard cb = Gtk.Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));

			if (cb.WaitIsImageAvailable ()) {
				PintaCore.Tools.Commit ();

				Gdk.Pixbuf image = cb.WaitForImage ();

				Layer l = PintaCore.Layers.AddNewLayer (string.Empty);

				using (Cairo.Context g = new Cairo.Context (l.Surface))
					g.DrawPixbuf (image, new Cairo.Point (0, 0));

				// Make new layer the current layer
				PintaCore.Layers.SetCurrentLayer (l);

				PintaCore.Workspace.Invalidate ();

				AddLayerHistoryItem hist = new AddLayerHistoryItem (Stock.Paste, Catalog.GetString ("Paste Into New Layer"), PintaCore.Layers.IndexOf (l));
				PintaCore.History.PushNewItem (hist);
			} else {
				ClipboardEmptyError ();
			}
		}

		private void ClipboardEmptyError ()
		{
			var primary = Catalog.GetString ("Paste cancelled");
			var secondary = Catalog.GetString ("The clipboard does not contain an image");
			var markup = "<span weight=\"bold\" size=\"larger\">{0}</span>\n\n{1}\n";
			markup = string.Format (markup, primary, secondary);

			var md = new MessageDialog (PintaCore.Chrome.MainWindow, DialogFlags.Modal,
						    MessageType.Error, ButtonsType.None, true,
						    markup);

			md.AddButton (Stock.Ok, ResponseType.Yes);

			md.Run ();
			md.Destroy ();
		}
	}
}
