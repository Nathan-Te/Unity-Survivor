using System;

[Flags]
public enum SpellTag
{
    None = 0,

    // Types de Mouvement (Mutuellement exclusifs en général)
    Projectile = 1 << 0, // Bolt
    Area = 1 << 1,       // Nova
    Smite = 1 << 2,      // Météore (Spawn sur cible)
    Orbit = 1 << 3,     // Orbit

    // Capacités supportées (Compatibilité)
    SupportsPierce = 1 << 4,    // Accepte le mod "Pierce" ?
    SupportsHoming = 1 << 5,    // Accepte le mod "Homing" ?
    SupportsMulticast = 1 << 6, // Accepte le mod "Multicast" ?
    SupportsDuration = 1 << 7,   // Accepte le mod "Durée" ?
    SupportsSizeChange = 1 << 8
}