using System;

[Flags]
public enum SpellTag
{
    None = 0,

    // Types de Mouvement (Mutuellement exclusifs en g�n�ral)
    Projectile = 1 << 0, // Bolt
    Area = 1 << 1,       // Nova
    Smite = 1 << 2,      // M�t�ore (Spawn sur cible)
    Orbit = 1 << 3,     // Orbit

    // Capacit�s support�es (Compatibilit�)
    SupportsPierce = 1 << 4,    // Accepte le mod "Pierce" ?
    SupportsHoming = 1 << 5,    // Accepte le mod "Homing" ?
    SupportsMultishot = 1 << 6, // Accepte le mod "Multishot" (ajoute des projectiles par cast) ?
    SupportsMulticast = 1 << 7, // Accepte le mod "Multicast" (répète le cast entier) ?
    SupportsDuration = 1 << 8,   // Accepte le mod "Dur�e" ?
    SupportsSizeChange = 1 << 9
}