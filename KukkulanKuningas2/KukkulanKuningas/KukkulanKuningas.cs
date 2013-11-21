using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class KukkulanKuningas : PhysicsGame
{
    PlatformCharacter2 pelaaja;
    PlatformCharacter vihu;
    int iHavitytPelit = 0;

    void LuoPelaaja(Vector paikka, double korkeus, double leveys)
    {
        pelaaja = new PlatformCharacter2(leveys/2, korkeus-1);
        pelaaja.Position = paikka;
        pelaaja.Color = Color.DarkForestGreen;
        pelaaja.IgnoresExplosions = true;
        pelaaja.KineticFriction = 1.0;
        Add(pelaaja);
    }
    void LuoVihollinen(Vector paikka, double korkeus, double leveys)
    {
        vihu = new PlatformCharacter(leveys / 2, korkeus - 1);
        vihu.Shape = Shape.Circle;
        vihu.Color = Color.Red;
        vihu.Position = paikka;
        vihu.Tag = "vihollinen";
        Add(vihu);
    }
    void LuoMaata(Vector paikka, double korkeus, double leveys)
    {
        PhysicsObject maa = PhysicsObject.CreateStaticObject(korkeus, leveys);
        maa.Position = paikka;
        maa.Tag = "maa";
        maa.CollisionIgnoreGroup = 1;
        maa.Color = Color.Brown;
        Add(maa);
    }
    void LuoReuna(Vector paikka, double korkeus, double leveys)
    {
        PhysicsObject maa = PhysicsObject.CreateStaticObject(korkeus, leveys);
        maa.Position = paikka;
        maa.CollisionIgnoreGroup = 1;
        Add(maa);
    }

    void LuoKentta()
    {
        ColorTileMap kentta = ColorTileMap.FromLevelAsset("kukkula");

        kentta.SetTileMethod(Color.Black, LuoReuna);
        kentta.SetTileMethod(Color.DarkGray, LuoMaata);
        kentta.SetTileMethod(Color.FromHexCode("00FF21"), LuoPelaaja);
        kentta.SetTileMethod(Color.Red, LuoVihollinen);
        kentta.Optimize(Color.Black);
        kentta.Execute(30, 30);

        Gravity = new Vector(0, -1000);
        AddCollisionHandler<PlatformCharacter2, PlatformCharacter>(pelaaja, "vihollinen", VihollinenKukkulalla);
    }

    void LuoOhjaimet()
    {
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape,
            ButtonState.Pressed,
            ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down,
            Liikuta, "Pelaaja liikkuu", Direction.Left);
        Keyboard.Listen(Key.Right, ButtonState.Down,
            Liikuta, "Pelaaja liikkuu", Direction.Right);
        Keyboard.Listen(Key.Up, ButtonState.Down,
            Hyppaa, "Pelaaja hyppaa", 400.0);
        Keyboard.Listen(Key.Space, ButtonState.Pressed,
            Ammu, "Ammu aseella");
    }

    void LuoAivot()
    {
        //List<Vector> polku = new List<Vector>();
        //polku.Add(new Vector(250, 1000));
        //FollowerBrain vihollisenAivot = new FollowerBrain(pelaaja);
        //RandomMoverBrain vihollisenAivot = new RandomMoverBrain();
        //PathFollowerBrain vihollisenAivot = new PathFollowerBrain();
        //LabyrinthWandererBrain vihollisenAivot = new LabyrinthWandererBrain(20);
        //vihollisenAivot.DistanceClose = 50;
        //vihollisenAivot.StopWhenTargetClose = true;
        //vihollisenAivot.TargetClose += VihollinenKukkulalla;
        //vihollisenAivot.Path = polku;

        PlatformWandererBrain vihollisenAivot = new PlatformWandererBrain();
        vihollisenAivot.JumpSpeed = 350;
        vihollisenAivot.TriesToJump = true;
        vihollisenAivot.Speed = 50;
        vihu.Brain = vihollisenAivot;
    }

    void Liikuta(Direction suunta)
    {
        pelaaja.Walk(suunta);
        if (pelaaja.X >= 10)
        {
            pelaaja.Stop();
            //pelaaja.StopWalking();
            pelaaja.X = 9;
        }
        if (pelaaja.X <= -10)
        {
            pelaaja.X = -9;
            pelaaja.Stop();
            //pelaaja.StopWalking();
        }
    }
    void Hyppaa(double korkeus)
    {
        pelaaja.Jump(korkeus);
    }

    void Ammu()
    {
        Grenade kranu = new Grenade(4.0, TimeSpan.FromSeconds(0.7));
        pelaaja.Throw(kranu, Angle.FromDegrees(10), 5000);
        kranu.Explosion.Force = 100;
        kranu.Explosion.MaxRadius = 70;
        kranu.Explosion.AddShockwaveHandler("vihollinen", KranuOsui);
    }

    void KranuOsui(IPhysicsObject rajahdyksenKohde, Vector v)
    {
        bool x = RandomGen.NextBool();
        if(x)
        {
            LuoVihollinen(new Vector(Level.Right - 40, Level.Bottom + 70), 30, 30);
        }
        else
        {
            LuoVihollinen(new Vector(Level.Left + 40, Level.Bottom + 70), 30, 30);
        }

        LuoAivot();
        rajahdyksenKohde.Destroy();
    }

    void VihollinenKukkulalla(PlatformCharacter2 tormaaja, PlatformCharacter kohde)
    {
        iHavitytPelit++;
        tormaaja.Destroy();
        String teksti;
        vihu.Color = Color.Azure;

        if (iHavitytPelit < 3)
        {
            Timer laskuri = new Timer();
            laskuri.Interval = 3;
            laskuri.Timeout += AloitaAlusta;
            laskuri.Start(1);
            teksti = "Hävisit " + iHavitytPelit + ". pelin!";
            Label tekstiKentta = new Label(400.0, 80.0, teksti);
            tekstiKentta.Font = Font.DefaultLarge;
            Add(tekstiKentta);
        }
        else
        {
            teksti = "Hävisit " + iHavitytPelit + ". pelin\n" + "Peli loppu siihen!";
            Label tekstiKentta = new Label(400.0, 80.0, teksti);
            tekstiKentta.Font = Font.DefaultLarge;
            Add(tekstiKentta);

            //teksti = "Hävisit " + iHavitytPelit + ". pelin";
            //MultiSelectWindow valikko = new MultiSelectWindow(teksti, "Uusi peli", "Lopeta");
            //valikko.ItemSelected += PainettiinNappia;
            //Add(valikko);
        }
    }

    void PainettiinNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                iHavitytPelit = 0;
                AloitaAlusta();
                break;
            case 1:
                Exit();
                break;
        }
    }

    void AloitaAlusta()
    {
        ClearAll();
        LuoKentta();
        LuoAivot();
        LuoOhjaimet();
    }
    
    public override void Begin()
    {
        LuoKentta();
        LuoAivot();
        LuoOhjaimet();
    }
}
