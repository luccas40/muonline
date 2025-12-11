using Client.Main.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Main.Controls.UI.Game.MasterSkillTree;

public class WindowCloseBtnControl : SpriteControl
{

    Texture2D _idle;
    Texture2D _hover;
    Texture2D _click;


    public WindowCloseBtnControl()
    {
        Interactive = true;
        ControlSize = ViewSize = new Point(16, 16);
        TextureRectangle = new(0, 0, 32, 32);
        BlendState = Blendings.Alpha;
    }

    public override async Task Load()
    {
        await base.Load();

        var tl = TextureLoader.Instance;
        _idle = await tl.PrepareAndGetTexture("Interface/GFx/btn_close01.ozd");
        _hover = await tl.PrepareAndGetTexture("Interface/GFx/btn_close02.ozd");
        _click = await tl.PrepareAndGetTexture("Interface/GFx/btn_close03.ozd");
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (IsMouseOver && IsMousePressed) // check IsMousePressed from GameControl for click visual
            SetTexture(_click);
        else if (IsMouseOver) // hover state
            SetTexture(_hover);
        else
            SetTexture(_idle);
        ViewSize = new Point(16, 16);
    }
}

