using Client.Data.BMD;
using Client.Main.Content;
using Client.Main.Controllers;
using Client.Main.Core.Utilities;
using Client.Main.Helpers;
using Client.Main.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Client.Main.Controls.UI.Game.MasterSkillTree;

public class MasterSkillBtnControl : UIControl, IUiTexturePreloadable
{
    Texture2D _bg;
    LabelControl _level;
    SpriteControl _skillIcon;
    private RenderTarget2D _staticSurface;
    private bool _staticSurfaceDirty = true;

    private MasterSkillTreeData? _masterData;

    public MasterSkillTreeData? MasterData
    {
        get => _masterData;
        set
        {
            _masterData = value;
            if (value == null)
            {
                Visible = false;
                _level.Text = "0";
                return;
            }
            var skillDef = SkillDatabase.GetSkillDefinition((int)_masterData.Value.SkillNum);
            var magicIcon = skillDef?.MagicIcon ?? 0;
            var x = 20 * (magicIcon % 25);
            var y = 28 * (magicIcon / 25);
            _skillIcon.TextureRectangle = new(x, y, 20, 28);
            Visible = true;
        }
    }

    public MasterSkillBtnControl()
    {
        Interactive = true;
        ControlSize = ViewSize = new Point(50, 44);
        _level = new LabelControl()
        {
            Text = "0",
            Align = ControlAlign.Bottom | ControlAlign.Right,
            FontSize = 9,
            Margin = new() { Bottom = -2, Right = 0 }
        };
        Controls.Add(_level);

        _skillIcon = new()
        {
            X = 5,
            Y = 4,
            ViewSize = new(25,36),
            TexturePath = "Interface/new_Master_Icon.OZJ",
            TextureRectangle = new(0, 0, 20, 28)
        };
        Controls.Add(_skillIcon);

    }
    public IEnumerable<string> GetPreloadTexturePaths() => ["Interface/GFx/masterskill_iconbox.ozd"];
    public override async Task Load()
    {
        await base.Load();
        var tl = TextureLoader.Instance;
        _bg = await tl.PrepareAndGetTexture("Interface/GFx/masterskill_iconbox.ozd");
        
    }

    public override bool OnClick()
    {
        base.OnClick();
        return true;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (IsMouseOver && IsMousePressed)
        {
            // check IsMousePressed from GameControl for click visual
        }
        else if (IsMouseOver)
        {
            // hover
        }
        else
        {
            // idle
        }
            
        
    }

    public override void Draw(GameTime gameTime)
    {
        if (!Visible)
        {
            return;
        }
        EnsureStaticSurface();

        var spriteBatch = GraphicsManager.Instance.Sprite;
        SpriteBatchScope scope = null;
        if (!SpriteBatchScope.BatchIsBegun)
        {
            scope = new SpriteBatchScope(spriteBatch, SpriteSortMode.Deferred, BlendState.AlphaBlend, transform: UiScaler.SpriteTransform);
        }

        try
        {
            if (_staticSurface != null && !_staticSurface.IsDisposed)
            {
                spriteBatch.Draw(_staticSurface, DisplayRectangle, Color.White * Alpha);
            }

            //DrawTexts(spriteBatch, font);
            //DrawButtons
            _level?.Draw(gameTime);
            _skillIcon?.Draw(gameTime);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    private void EnsureStaticSurface()
    {
        if (!_staticSurfaceDirty && _staticSurface != null && !_staticSurface.IsDisposed)
        {
            return;
        }

        var graphicsDevice = GraphicsManager.Instance.GraphicsDevice;
        if (graphicsDevice == null)
        {
            return;
        }

        _staticSurface?.Dispose();
        _staticSurface = new RenderTarget2D(graphicsDevice, 64, 64, false, SurfaceFormat.Color, DepthFormat.None);

        var previousTargets = graphicsDevice.GetRenderTargets();
        graphicsDevice.SetRenderTarget(_staticSurface);
        graphicsDevice.Clear(Color.Transparent);

        var spriteBatch = GraphicsManager.Instance.Sprite;
        using (new SpriteBatchScope(spriteBatch, SpriteSortMode.Deferred, BlendState.NonPremultiplied))
        {
            spriteBatch.Draw(_bg, new Rectangle(0, 0, 64, 64), Color.White * Alpha);
        }

        graphicsDevice.SetRenderTargets(previousTargets);
        _staticSurfaceDirty = false;
    }


}
