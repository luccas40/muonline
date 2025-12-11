using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Client.Main.Controls.UI;

public class ButtonS8 : SpriteControl
{

    //private Rectangle _hover = new Rectangle(1, 0, 75, 36);
    private Rectangle _hover = new Rectangle(232, 0, 75, 36);
    private Rectangle _idle = new Rectangle(78, 0, 75, 36);
    private Rectangle _click = new Rectangle(155, 0, 75, 36);

    private LabelControl _label;

    private string _text;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _label?.Text = value;
        }
    }

    public bool Enabled { get; set; } = true;

    public ButtonS8()
    {
        Interactive = true;
        ViewSize = new Point(75, 36);
        BlendState = Blendings.Alpha;
        TexturePath = "Interface/GFx/Popups_I1.ozd";
        TextureRectangle = _idle;
    }

    public override async Task Initialize()
    {
        Controls.Add(_label = new()
        {
            Text = _text,
            Align = Models.ControlAlign.HorizontalCenter | Models.ControlAlign.VerticalCenter
        });
        await base.Initialize();
    }

    public override bool OnClick()
    {
        if (Enabled)
        {
            base.OnClick();
            return true;
        }
        return false;
    }


    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (IsMouseOver && IsMousePressed) // check IsMousePressed from GameControl for click visual
            TextureRectangle = _click;
        else if (IsMouseOver) // hover state
            TextureRectangle = _hover;
        else
            TextureRectangle = _idle;
    }
}
