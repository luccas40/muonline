using Client.Data.BMD;
using Client.Main.Content;
using Client.Main.Controllers;
using Client.Main.Helpers;
using Client.Main.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Client.Main.Controls.UI.Game.MasterSkillTree;

public class CharacterMasterClassWindowControl : UIControl, IUiTexturePreloadable
{
    private const int WINDOW_WIDTH = 916;
    private const int WINDOW_HEIGHT = 688;

    private static readonly string[] s_tableTexturePaths =
    {
        "Interface/GFx/MasterSkillTree_I1.ozd",
    };

    public IEnumerable<string> GetPreloadTexturePaths() => s_tableTexturePaths;

    private Texture2D _testBg;

    private RenderTarget2D _staticSurface;
    private bool _staticSurfaceDirty = true;

    LabelControl _className;
    LabelControl _level;
    LabelControl _points;
    LabelControl _exp;
    LabelControl _determination;
    LabelControl _justice;
    LabelControl _conquer;
    WindowCloseBtnControl _closeBtn;
    MasterSkillBtnControl[] _skillBtns;
    ConfirmationDialog _improveSkillDialog;
    private Dictionary<ushort, List<MasterSkillTreeData>> _masterData;

    public CharacterMasterClassWindowControl()
    {
        ControlSize = new Point(WINDOW_WIDTH, WINDOW_HEIGHT);
        ViewSize = ControlSize;
        AutoViewSize = false;
        Interactive = true;
        Visible = false;
    }

    public override async Task Load()
    {
        await base.Load();

        var tl = TextureLoader.Instance;

        _testBg = await tl.PrepareAndGetTexture("Interface/GFx/MasterSkillTree_I1.ozd");
        _masterData = BMDTextReader.Read<MasterSkillTreeData>(Path.Combine(Constants.DataPath, "Local", "masterskilltreedata.bmd"), itemCounter: false).GroupBy(d => d.Class).ToDictionary(i => i.Key, i => i.ToList());
        InitializeLayout();
    }

    private void InitializeLayout()
    {
        _closeBtn = new()
        {
            Align = ControlAlign.Top | ControlAlign.Right,
        };
        _closeBtn.Margin = new() { Top = 40, Right = 40 };
        Controls.Add(_closeBtn);

        _className = new()
        {
            Align = ControlAlign.Top | ControlAlign.HorizontalCenter,
            Text = "Empire Lord",
            TextColor = Color.Gold,
            Margin = new() { Top = 40, Left = -180 }
        };
        Controls.Add(_className);

        _level = new()
        {
            Align = ControlAlign.Top | ControlAlign.HorizontalCenter,
            Text = "Level: 802",
            Margin = new() { Top = 40, Left = 30 }
        };
        Controls.Add(_level);

        _points = new()
        {
            Align = ControlAlign.Top | ControlAlign.HorizontalCenter,
            Text = "Points: 36",
            Margin = new() { Top = 40, Left = 165 }
        };
        Controls.Add(_points);

        _exp = new()
        {
            Align = ControlAlign.Top | ControlAlign.HorizontalCenter,
            Text = "EXP: 59.01%",
            Margin = new() { Top = 40, Left = 310 }
        };
        Controls.Add(_exp);

        _determination = new()
        {
            Align = ControlAlign.Top | ControlAlign.HorizontalCenter,
            Text = "Determination: 103",
            TextColor = Color.Gold,
            Margin = new() { Top = 80, Left = -290 }
        };
        Controls.Add(_determination);

        _justice = new()
        {
            Align = ControlAlign.Top | ControlAlign.HorizontalCenter,
            Text = "Justice: 103",
            TextColor = Color.Gold,
            Margin = new() { Top = 80, Left = 0 }
        };
        Controls.Add(_justice);

        _conquer = new()
        {
            Align = ControlAlign.Top | ControlAlign.HorizontalCenter,
            Text = "Conquer: 103",
            TextColor = Color.Gold,
            Margin = new() { Top = 80, Left = 290 }
        };
        Controls.Add(_conquer);

        _skillBtns = new MasterSkillBtnControl[4 * 9 * 3];
        for(int i = 0; i < 4*9*3; i++)
        {
            int type = i / (4 * 9);
            int index = i - (4 * 9 * type);
            int column = index % 4;
            int row = index / 4;
            _skillBtns[i] = new MasterSkillBtnControl()
            {
                X = 30 + column * (32 + 38) + type * 296,
                Y = 114 + row * (32 + 30), //half size + padding 18
                Visible = false,
            };
            Controls.Add(_skillBtns[i]);
        }
        //_skillBtns[0].Visible = true;
    }


    public void InitializeClassMasterData(int classId)
    {
        if(!_masterData.TryGetValue((ushort)classId, out var data))
        {
            return;
        }
        
        foreach (var item in _skillBtns)
        {
            item.MasterData = null;
        }

        foreach (var item in data)
        {
            var btn = _skillBtns[item.ID - 1];
            btn.MasterData = item;
            btn.Click += masterSkillBtn_OnClick;
        }
    }

    private void masterSkillBtn_OnClick(object sender, EventArgs e)
    {
        if (_improveSkillDialog != null) return;
        MasterSkillBtnControl btn = sender as MasterSkillBtnControl;
        _improveSkillDialog = ConfirmationDialog.Show("", $"Would you like to strengthen the skill?\n    Master level point requirement: {btn.MasterData.Value.ReqPoint}", 
            () => {
                //send server request add skill
                _improveSkillDialog = null;
            }, 
            () => {
                _improveSkillDialog = null;
            });        
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
            _className.Draw(gameTime);
            _level.Draw(gameTime);
            _points.Draw(gameTime);
            _exp.Draw(gameTime);
            _determination.Draw(gameTime);
            _justice.Draw(gameTime);
            _conquer.Draw(gameTime);
            _closeBtn.Draw(gameTime);
            
            for (int i = 0; i < _skillBtns.Length; i++)
            {
                var btn = _skillBtns[i];
                if (!btn.Visible) continue;
                btn.Draw(gameTime);
            }
            //_improveSkillDialog?.Draw(gameTime);
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
        _staticSurface = new RenderTarget2D(graphicsDevice, WINDOW_WIDTH, WINDOW_HEIGHT, false, SurfaceFormat.Color, DepthFormat.None);

        var previousTargets = graphicsDevice.GetRenderTargets();
        graphicsDevice.SetRenderTarget(_staticSurface);
        graphicsDevice.Clear(Color.Transparent);

        var spriteBatch = GraphicsManager.Instance.Sprite;
        using (new SpriteBatchScope(spriteBatch, SpriteSortMode.Deferred, BlendState.NonPremultiplied))
        {
            DrawStaticElements(spriteBatch);
        }

        graphicsDevice.SetRenderTargets(previousTargets);
        _staticSurfaceDirty = false;
    }

    private void DrawStaticElements(SpriteBatch spriteBatch)
    {

        if(_testBg != null)
        {
            // top left
            spriteBatch.Draw(_testBg, new Rectangle(0, 0, WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), new Rectangle(464, 1, 465, 355), Color.White);

            // top right
            spriteBatch.Draw(_testBg, new Rectangle(WINDOW_WIDTH / 2, 0, WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), new Rectangle(0,360, 463, 355), Color.White);            

            // bottom left
            spriteBatch.Draw(_testBg, new Rectangle(0, WINDOW_HEIGHT / 2, WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), new Rectangle(464, 358, 464, 355), Color.White);

            // bottom right
            spriteBatch.Draw(_testBg, new Rectangle(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2, WINDOW_WIDTH / 2, 1 + WINDOW_HEIGHT / 2), new Rectangle(0, 0, 463, 355), Color.White);
        }
    }

    private void InvalidateStaticSurface()
    {
        _staticSurfaceDirty = true;
    }

    public void ShowWindow()
    {
        Visible = true;
        /*
        _pressedButtonIndex = -1;
        _hoveredButtonIndex = -1;
        _hasCachedSnapshot = false;
        UpdateDisplayData();*/
        InvalidateStaticSurface();
        BringToFront();
        SoundController.Instance.PlayBuffer("Sound/iCreateWindow.wav");
        if (Scene != null)
        {
            Scene.FocusControl = this;
        }
    }

    public void HideWindow()
    {
        Visible = false;
        if (Scene != null && Scene.FocusControl == this)
        {
            Scene.FocusControl = null;
        }
        if(_improveSkillDialog != null)
        {
            _improveSkillDialog.Close();
            _improveSkillDialog = null;
        }
    }
}
