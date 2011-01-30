using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace lightseeds.HUD
{
  public class Minimap
  {
    private Texture2D Back;
    private Texture2D Front;
    private Texture2D Fruit;
    private Texture2D Player1;
    private Texture2D Player2;

    private float percentage = 0.1f; // between 0.0 and 1.0
    public Vector2 Player1Pos = new Vector2(50, 0);
    public Vector2 Player2Pos = new Vector2(-150, 5);
    public List<Vector2> Fruits = new List<Vector2>();


    private SpriteBatch spriteBatch;
    private RenderTarget2D renderTarget;

    private GraphicsDevice device;

    private Effect MinimapEffect;

    public Minimap(SpriteBatch sb, ContentManager content, GraphicsDevice gd)
    {
      spriteBatch = sb;
      device = gd;

      Back = content.Load<Texture2D>("Minimap/MinimapBack");
      Front = content.Load<Texture2D>("Minimap/MinimapFront");
      Fruit = content.Load<Texture2D>("Minimap/MinimapFruit");
      Player1 = content.Load<Texture2D>("Minimap/MinimapPlayer1");
      Player2 = content.Load<Texture2D>("Minimap/MinimapPlayer2");

      MinimapEffect = content.Load<Effect>("effects/MinimapEffect");
      MinimapEffect.Parameters["Darkness"].SetValue(Front);
      
      
      renderTarget = new RenderTarget2D(gd, Back.Width, Back.Height);
      Random r = new Random();


      for (int i = 0; i < 10; i++)
        Fruits.Add(new Vector2(r.Next(-200, 200), r.Next(0, 5)));
    }

    public void SetPercentage(float P)
    {
      if (P > 1.0f)
        P = 1.0f;
      else if (P < 0.0f)
        P = 0.0f;

      percentage = P;
    }

    public Texture2D Render()
    {
      MinimapEffect.Parameters["Percent"].SetValue(percentage);
      device.SetRenderTarget(renderTarget);

      spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, MinimapEffect);
      spriteBatch.Draw(Back, new Vector2(0.0f, 0.0f), new Color(255,255,255,0));
      spriteBatch.End();

      DrawObjects();

      return renderTarget;
    }

    private void DrawObjects()
    {
      Vector2 p1 = CalculatePos(Player1Pos);
      Vector2 p2 = CalculatePos(Player2Pos);

      spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

      foreach (Vector2 pos in Fruits)
      {
        Vector2 newPos = CalculatePos(pos);
        spriteBatch.Draw(Fruit, newPos, Color.White);
      }

      spriteBatch.Draw(Player1, p1, Color.White);
      spriteBatch.Draw(Player2, p2, Color.White);
      
      spriteBatch.End();
    }

    private Vector2 CalculatePos(Vector2 pos)
    {
      double Angle = (1 - pos.X / 200) * Math.PI;
      return new Vector2((float)(96 + (70 + pos.Y) * Math.Sin(Angle)), (float)(96 + (70 + pos.Y) * Math.Cos(Angle)));
    }
  }
}
