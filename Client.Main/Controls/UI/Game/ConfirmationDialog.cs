using Client.Main.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Main.Controls.UI.Game;

public class ConfirmationDialog : PopupFieldDialog
{
    private readonly SpriteControl _line1;
    private readonly SpriteControl _line2;

    private readonly LabelControl _title;
    private readonly LabelControl _description;


    public ConfirmationDialog()
    {
        Interactive = true; // consume mouse interactions over the dialog
        ControlSize = new Point(300, 200);
        Controls.Add(_title = new LabelControl
        {
            Text = "",
            Align = ControlAlign.HorizontalCenter,
            Y = 15,
            FontSize = 14
        });

        Controls.Add(_line1 = new SpriteControl
        {
            TexturePath = "Interface/GFx/popup_line_m.ozd",
            TextureRectangle = new(0, 0, 512, 16),
            Align = ControlAlign.HorizontalCenter,
            Y = 40,
            ViewSize = new(ControlSize.X - 30, 4)
            //AutoViewSize = true
        });

        Controls.Add(_description = new LabelControl
        {
            Text = "",
            Align = ControlAlign.HorizontalCenter,
            Y = 55,
            FontSize = 12,
            TextColor = Color.LightGray
        });

        Controls.Add(_line2 = new SpriteControl
        {
            TexturePath = "Interface/GFx/popup_line_m.ozd",
            TextureRectangle = new(0, 0, 512, 16),
            Align = ControlAlign.HorizontalCenter | ControlAlign.Bottom,
            Margin = new() { Bottom = 60 },
            ViewSize = new(ControlSize.X - 30, 2),
            //BorderColor = Color.Yellow,
            //BorderThickness = 4
            //AutoViewSize = true
        });


    }

    // Consume clicks anywhere on the dialog (background or label), so they don't reach the world.
    public override bool OnClick()
    {
        return true; // don't propagate; buttons will handle their own click events
    }


    public static ConfirmationDialog Show(string title, string description, Action? confirm = null, Action? cancel = null, string confirmText = "OK", string cancelText = "Cancel")
    {
        var scene = MuGame.Instance?.ActiveScene;
        if (scene == null)
        {
            return null;
        }

        ConfirmationDialog dialog = new();
        dialog._title.Text = title;
        dialog._description.Text = description;

        if(confirm != null && cancel != null)
        {
            ButtonS8 btn;
            dialog.Controls.Add(btn = new ButtonS8()
            {
                Text = confirmText,
                Align = ControlAlign.HorizontalCenter | ControlAlign.Bottom,
                Margin = new() { Bottom = 15, Right = 40 },
            });
            btn.Click += (sender, e) => { confirm.Invoke(); dialog.Close(); };

            ButtonS8 btn2;
            dialog.Controls.Add(btn2 = new ButtonS8()
            {
                Text = cancelText,
                Align = ControlAlign.HorizontalCenter | ControlAlign.Bottom,
                Margin = new() { Bottom = 15, Left = 40 },
            });
            btn2.Click += (sender, e) => { cancel.Invoke(); dialog.Close(); };
        }
        else
        {
            ButtonS8 btn;
            dialog.Controls.Add(btn = new ButtonS8()
            {
                Text = confirmText,
                Align = ControlAlign.HorizontalCenter | ControlAlign.Bottom,
                Margin = new() { Bottom = 15 },
            });
            btn.Click += (sender, e) => { confirm?.Invoke(); dialog.Close(); };
        }

        dialog.ShowDialog();
        dialog.BringToFront();
        return dialog;
    }
}
