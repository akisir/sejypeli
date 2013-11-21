/***Meteori tippuu, peliohjelmointikurssin esimerkki törmäyksistä by Aki Sirviö***/
using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class Tormaukset : PhysicsGame
{
    Surface alaReuna;
    PhysicsObject kuu;
    Cannon tykki;
    PhysicsObject meteori1;
    PhysicsObject meteori2;
    bool kuuKasvaa = true;
    
    //Peli alkaa aina Begin aliohjelmasta, josta kutsutaan muita aliohjelmia
    public override void Begin()
    {
        LisaaAlaReuna();
        LisaaOhjaimet();
        LisaaTykki();
        LisaaKuu();
        LisaaAjastin1();
        LisaaAjastin2();
        
        Gravity = new Vector(0.0,-20.0);
        Camera.ZoomFactor = 1.2;
        //Camera.ZoomToAllObjects();
    }

    //Tehdään pelikentälle alareuna eli vähän maan pintaa
    void LisaaAlaReuna()
    {
        alaReuna = Surface.CreateBottom(Level, 15, 100, 50, 15);
        Add(alaReuna);

        //Asetetaan törmäyksien käsittelijä ja tuhotaan kaikki alareunaan osuvat PhysicsObject -oliot
        AddCollisionHandler(alaReuna, CollisionHandler.ExplodeTarget(150, true));

        //AddCollisionHandler(alaReuna, CollisionHandler.DestroyTarget);
    }

    //Asetetaan näppäimistön komennot
    void LisaaOhjaimet()
    {
        Keyboard.Listen(Key.Space, ButtonState.Down, Ammu, "Ammu aseella");
        Keyboard.Listen(Key.A, ButtonState.Down, KaannaTykki, "Käännä tykkiä vasemmalle", 1);
        Keyboard.Listen(Key.D, ButtonState.Down, KaannaTykki, "Käänna tykkiä oikealle", -1);
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaTykki, "Liikuta tykkiä vasemmalle", -1);
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaTykki, "Liikuta tykkiä oikealle", 1);
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    //Ajastetaan meteori1:n tippuminen
    void LisaaAjastin1()
    {
        Timer ajastin = new Timer();
        ajastin.Interval = 0.8;
        ajastin.Timeout += TiputaMeteori1;
        ajastin.Start();
    }

    //Ajastetaan meteori2:n tippuminen
    void LisaaAjastin2()
    {
        Timer ajastin = new Timer();
        ajastin.Interval = 1.3;
        ajastin.Timeout += TiputaMeteori2;
        ajastin.Start();
    }

    //Lisätään taivaalle kuu
    void LisaaKuu()
    {
        kuu = new PhysicsObject(70, 70, Shape.Circle);
        kuu.X = Level.Left + 10;
        kuu.Y = Level.Top - 70;
        kuu.Color = Color.Wheat;
        kuu.IgnoresCollisionResponse = true;
        kuu.IgnoresExplosions = true;
        kuu.IgnoresGravity = true;
        Add(kuu);

        //Asetetaan törmäyksien käsittelijä, joka kutsuu MeteoriOsuuKuuhun -aliohjelmaa
        AddCollisionHandler(kuu, "meteori1", MeteoriOsuuKuuhun);

        //AddCollisionHandler(kuu, MeteoriOsuuKuuhun);
        //AddCollisionHandler(kuu,CollisionHandler.IncreaseObjectSize(10,10));
    }

    //Lisätään tykki maan pinnalle
    void LisaaTykki()
    {
        tykki = new Cannon(100, 20);
        tykki.Angle = Angle.FromDegrees(90);
        tykki.Power.DefaultValue = 9000;
        tykki.FireRate = 10.0;
        //tykki.Y = Level.Bottom;
        tykki.Y = alaReuna.Top + 5;
        tykki.AmmoIgnoresGravity = false;
        tykki.AmmoIgnoresExplosions = true;

        //Asetetaan ammukselle törmäyksien käsittelijä
        //tykki.ProjectileCollision = CollisionHandler.DestroyBoth;
        //tykki.ProjectileCollision = CollisionHandler.ExplodeTarget(100,true);
        //tykki.ProjectileCollision = CollisionHandler.ShowMessage("OSUI");
        tykki.ProjectileCollision = AmmusOsuu;

        Add(tykki);
    }

    //Space näppäintä on painettu ja ammutaan tykillä
    void Ammu()
    {
        tykki.Shoot();
    }

    //A- tai D-näppäintä on painettu ja käännetään tykkiä
    void KaannaTykki(int aste)
    {
        tykki.Angle += Angle.FromDegrees(aste);
        //tykki.OscillateAngle(aste, UnlimitedAngle.FromDegrees(25.0), 2.0, 0.0);
    }

    //Nuolinäppäintä(Left,Right) on painettu ja liikutetaan tykkiä
    void LiikutaTykki(int suunta)
    {
        tykki.X = tykki.X + suunta;
        //tykki.Move(new Vector(suunta, 0));
        
        /*if (suunta == 1)
        {
            tykki.MoveTo(new Vector(alaReuna.Right, alaReuna.Top + 5), 300);
        }
        else
        {
            tykki.MoveTo(new Vector(alaReuna.Left, alaReuna.Top + 5), 300);
        }*/

        //tykki.Oscillate(Vector.UnitX, 100 * suunta, 2);
        //tykki.Oscillate(Vector.UnitX, 100*suunta, 2, 0, 1);
    }

    //Ammus on osunut ja törmäyksien käsittelijä kutsuu AmmusOsuu -aliohjelmaa
    void AmmusOsuu(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        //Meteori1 pysäytetään ja väriä muutetaan
        if (kohde.Tag == "meteori1")
        {
            kohde.Color = RandomGen.NextColor();
            kohde.Stop();
            //kohde.Destroy();
            kohde.IgnoresGravity = true;
        }
        //Meteori2 räjäytetään
        if (kohde.Tag == "meteori2")
        {
            Explosion rajahdys = new Explosion(70);
            rajahdys.Position = kohde.Position;
            Add(rajahdys);
            tormaaja.Destroy();
            kohde.Destroy();
        }
        //Kuun väri muutetaan keltaiseksi ja sen kokoa muutetaan
        if (kohde == kuu)
        {
            kohde.Color = Color.Yellow;
            tormaaja.Destroy();
            if (kohde.Width < 500 && kuuKasvaa)
            {
                kohde.Size = new Vector(kohde.Width + 10, kohde.Height + 10);
            }
            else if (kohde.Width > 20)
            {
                kuuKasvaa = false;
                kohde.Size = new Vector(kohde.Width - 10, kohde.Height - 10);
            }
            else
            {
                kuuKasvaa = true;
            }
        }
    }

    //Kun kuu törmää meteoriin, muutetaan sen väriä randomilla
    void MeteoriOsuuKuuhun(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        tormaaja.Color = RandomGen.NextColor();
    }

    //Ajastin1 kutsuu tätä aliohjelmaa tietyin väliajoin ja tiputtaa meteorin
    void TiputaMeteori1()
    {
        int koko = RandomGen.NextInt(20, 50);
        meteori1 = new PhysicsObject(koko, koko, Shape.Circle);
        //GameObject meteori1 = new GameObject(koko, koko, Shape.Circle);
        //PlatformCharacter meteori1 = new PlatformCharacter(koko, koko, Shape.Circle);
        //Tank meteori1 = new Tank(koko, koko);
        meteori1.X = RandomGen.NextDouble(Level.Left, Level.Right);
        meteori1.Y = Level.Top;
        meteori1.Tag = "meteori1";
        meteori1.IgnoresExplosions = true;
        meteori1.IgnoresCollisionResponse = true;
        Add(meteori1);
    }

    //Ajastin2 kutsuu tätä aliohjelmaa tietyin väliajoin ja tiputtaa meteorin
    void TiputaMeteori2()
    {
        meteori2 = new PhysicsObject(45, 45, Shape.Circle);
        meteori2.Color = Color.Black;
        meteori2.X = RandomGen.NextDouble(Level.Left, Level.Right);
        meteori2.Y = Level.Top;
        meteori2.Tag = "meteori2";
        meteori2.IgnoresExplosions = true;
        meteori2.IgnoresCollisionResponse = true;
        Add(meteori2);
    }
}
