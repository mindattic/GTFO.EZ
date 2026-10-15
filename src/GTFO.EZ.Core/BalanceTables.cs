namespace GTFO.EZ.Core;

/// <summary>
/// Static game-balance data the plugin turns into config entries and lookup keys. Pulled out of
/// <c>Plugin.cs</c> so it can be validated by <c>GTFO.EZ.Core.Tests</c> without needing BepInEx or
/// the game's own assemblies - see <c>docs/USER_STORIES.md</c> tickets GE-T3/GE-T4.
/// </summary>
public static class BalanceTables
{
    // MeleeArchetypeDataBlock - one entry per melee weapon type: Bat, Hammer, Knife, Spear (real
    // in-game names, confirmed from PublicName - "Hammer" covers what's commonly called the
    // sledgehammer). Organized by archetype, then by action type (Light/Charged/Push/General),
    // so with 4 archetypes x 19 stats each this is a lookup table rather than 76 named fields.
    public static readonly string[] MeleeArchetypes = { "Bat", "Hammer", "Knife", "Spear" };

    // FieldKey matches the MeleeArchetypeDataBlock property name this multiplies, so
    // GameDataTweaks can look it up directly without a second mapping table.
    public static readonly (string FieldKey, string ActionType, string DisplayName, string Description)[] MeleeStatDefs =
    {
        ("LightAttackDamage", "Light Attack", "Damage Multiplier", "Multiplies light-attack damage."),
        ("LightStaggerMulti", "Light Attack", "Stagger Multiplier", "Multiplies stagger force from a light attack."),
        ("LightPrecisionMulti", "Light Attack", "Precision Damage Multiplier", "Multiplies the weak-point damage bonus on a light attack."),
        ("LightEnvironmentMulti", "Light Attack", "Environment Damage Multiplier", "Multiplies light-attack damage dealt to doors/props/environment."),
        ("LightBackstabberMulti", "Light Attack", "Backstab Damage Multiplier", "Multiplies the backstab damage bonus on a light attack."),
        ("LightSleeperMulti", "Light Attack", "Sleeper Damage Multiplier", "Multiplies the sleeping-enemy damage bonus on a light attack."),
        ("LightAttackStaminaCost", "Light Attack", "Stamina Cost Multiplier", "Multiplies stamina cost for a light attack."),

        ("ChargedAttackDamage", "Charged Attack", "Damage Multiplier", "Multiplies charged (heavy) attack damage."),
        ("ChargedStaggerMulti", "Charged Attack", "Stagger Multiplier", "Multiplies stagger force from a charged attack."),
        ("ChargedPrecisionMulti", "Charged Attack", "Precision Damage Multiplier", "Multiplies the weak-point damage bonus on a charged attack."),
        ("ChargedEnvironmentMulti", "Charged Attack", "Environment Damage Multiplier", "Multiplies charged-attack damage dealt to doors/props/environment."),
        ("ChargedBackstabberMulti", "Charged Attack", "Backstab Damage Multiplier", "Multiplies the backstab damage bonus on a charged attack."),
        ("ChargedSleeperMulti", "Charged Attack", "Sleeper Damage Multiplier", "Multiplies the sleeping-enemy damage bonus on a charged attack."),
        ("ChargedAttackStaminaCost", "Charged Attack", "Stamina Cost Multiplier", "Multiplies stamina cost for a charged attack."),

        ("PushDamageSphereRadius", "Push", "Hit Detection Radius Multiplier", "Multiplies the hit-detection radius of the push attack."),
        ("PushStaminaCost", "Push", "Stamina Cost Multiplier", "Multiplies stamina cost for a push."),

        ("CameraDamageRayLength", "General", "Reach Multiplier", "Multiplies attack reach."),
        ("AttackSphereRadius", "General", "Hit Detection Radius Multiplier", "Multiplies the hit-detection radius of light and charged attacks."),
        ("PlayerRunSpeedMultiWhileCharging", "General", "Charge Move Speed Multiplier", "Multiplies move speed while charging a heavy attack."),
    };

    // ArchetypeDataBlock - ranged weapons (guns and sentries share this block type). Core combat
    // stats get a two-tier multiplier instead of melee's single tier, because ranged weapons span
    // ~20 types and ~50 individual fire-mode archetypes rather than melee's 4: a weapon TYPE slider
    // (shared by every fire-mode variant in that family, e.g. all of "Rifle") multiplied by an
    // INDIVIDUAL slider (one specific archetype, e.g. "Rifle - Burst"), both default 1.0 and
    // cumulative. Category/archetype IDs, ownership, and vanilla stats confirmed by logging
    // GameDataBlockBase<GearCategoryDataBlock>/<ArchetypeDataBlock>.GetAllBlocks() at boot - not
    // documented anywhere official. An archetype belongs to whichever named GearCategory first
    // claims it (categories can share archetypes between fire-mode slots, e.g. "SMG_Special" reuses
    // plain "SMG"'s Semi/Burst/Auto archetypes, so only the first/base category exposes a slider for
    // them). Sentries and other tools don't route through GearCategoryDataBlock at all, so they
    // aren't covered by this two-tier system - only their flat multipliers below apply to them.
    public static readonly string[] WeaponTypeCategories =
    {
        "Bullpup", "DMR", "Gauss_Rifle", "HEL_Shotgun", "HighCalPistol", "MachineGun",
        "MachineGun_LowRPM", "Pistol", "Pistol_Stagger", "Precision_Rifle", "Revolver", "Rifle",
        "RifleCQB", "RifleHeavy", "SMG", "SMGHeavy", "SMG_PDW", "SawedOffShotgun", "Scattergun",
        "Shotgun", "Sniper",
    };

    public static readonly (uint ArchetypeId, string TypeCategory, string Label)[] WeaponArchetypes =
    {
        (51, "Bullpup", "Bullpup Semi"),
        (50, "Bullpup", "Bullpup Burst"),
        (49, "Bullpup", "Bullpup Auto"),
        (53, "Bullpup", "Bullpup SemiBurst"),
        (25, "DMR", "DMR Semi"),
        (26, "DMR", "DMR Burst"),
        (27, "DMR", "DMR Auto (Long gun)"),
        (28, "DMR", "DMR SemiBurst"),
        (65, "Gauss_Rifle", "Gauss Rifle Semi"),
        (81, "HEL_Shotgun", "HEL Shotgun Semi"),
        (78, "HEL_Shotgun", "HEL Shotgun Burst"),
        (80, "HighCalPistol", "HighCalPistol"),
        (59, "MachineGun", "MachineGun Auto"),
        (21, "MachineGun_LowRPM", "MachineGun LowRPM Semi"),
        (22, "MachineGun_LowRPM", "MachineGun LowRPM Burst"),
        (23, "MachineGun_LowRPM", "MachineGun LowRPM Auto"),
        (24, "MachineGun_LowRPM", "MachineGun LowRPM SemiBurst"),
        (41, "Pistol", "Pistol Semi"),
        (42, "Pistol", "Pistol Burst"),
        (43, "Pistol", "Pistol Auto"),
        (44, "Pistol", "Pistol SemiBurst"),
        (66, "Pistol_Stagger", "Pistol Stagger Auto"),
        (79, "Precision_Rifle", "Precision Rifle"),
        (37, "Revolver", "Revolver Semi"),
        (38, "Revolver", "Revolver Burst"),
        (39, "Revolver", "Revolver Auto (Autorevolver)"),
        (40, "Revolver", "Revolver SemiBurst"),
        (1, "Rifle", "Rifle Semi"),
        (3, "Rifle", "Rifle Burst"),
        (5, "Rifle", "Rifle Auto"),
        (16, "Rifle", "Rifle SemiBurst (Semi Burst Rifle)"),
        (76, "RifleCQB", "RifleCQB"),
        (73, "RifleHeavy", "RifleHeavy Auto"),
        (17, "SMG", "SMG Semi"),
        (18, "SMG", "SMG Burst"),
        (19, "SMG", "SMG Auto"),
        (20, "SMG", "SMG SemiBurst"),
        (69, "SMGHeavy", "SMGHeavy Auto"),
        (77, "SMG_PDW", "SMG PDW"),
        (45, "SawedOffShotgun", "SawedOffShotgun Semi"),
        (72, "Scattergun", "Scattergun Burst"),
        (33, "Shotgun", "Shotgun Semi"),
        (34, "Shotgun", "Shotgun Burst"),
        (35, "Shotgun", "Shotgun Auto"),
        (36, "Shotgun", "Shotgun SemiBurst"),
        (29, "Sniper", "Sniper Semi"),
        (30, "Sniper", "Sniper Burst (Railgun)"),
        (31, "Sniper", "Sniper Auto (Coilgun)"),
        (32, "Sniper", "Sniper SemiBurst"),
    };

    // Real in-game name of the one player weapon that uses each archetype. Resolved from a vanilla
    // datablock dump (game build 34855) the same way the game does it - offline gear's category +
    // fire mode -> that GearCategory's Semi/Burst/Auto/SemiBurst archetype - and cross-checked
    // against gtfo.wiki.gg. Archetypes missing here have no player-facing weapon. The Label above
    // stays the config section name regardless, so renaming never orphans a player's settings;
    // note a Label's fire-mode word is the archetype slot, not always how the gun actually fires
    // (81 is a slug shotgun under HEL_Shotgun, 78 fires full-auto from the Burst slot).
    public static readonly IReadOnlyDictionary<uint, string> WeaponNames = new Dictionary<uint, string>
    {
        [1] = "Drekker Pres MOD 556 Rifle",
        [3] = "Malatack CH 4 Burst Rifle",
        [5] = "Malatack LX Assault Rifle",
        [17] = "Drekker CLR Short Rifle",
        [18] = "Van Auken CAB F4 Carbine",
        [19] = "Van Auken LTC5 SMG",
        [21] = "Omneco EXP1 HEL Gun",
        [22] = "TechMan Klust 6 Burst Cannon",
        [23] = "TechMan Arbalist V Machine Gun",
        [25] = "TR22 Hanaway DMR",
        [26] = "Hanaway PSB Double Tap Rifle",
        [29] = "Köning PR 11 Sniper",
        [33] = "Buckland S870 Shotgun",
        [34] = "Buckland XDIST2 Choke Mod Shotgun",
        [35] = "Buckland AF6 Combat Shotgun",
        [37] = "Mastaba R66 Revolver",
        [38] = "Bataldo 3RB HEL Revolver",
        [41] = "Shelling S49 Pistol",
        [42] = "Shelling Nano Burst Pistol",
        [43] = "Raptus Treffen 2 Machine Pistol",
        [45] = "Buckland SBS III Sawed-off Shotgun",
        [49] = "Accrat Golok DA Bullpup Rifle",
        [59] = "TechMan Veruta XII Machine Gun",
        [65] = "Omneco LRG HEL Rifle",
        [66] = "Raptus Steigro HEL Autopistol",
        [69] = "Accrat ND6 Heavy SMG",
        [72] = "Drekker INEX Drei Scattergun",
        [73] = "Malatack HXC Heavy Assault Rifle",
        [77] = "Accrat STB PDW",
        [78] = "Bataldo J 300 HEL Shotgun",
        [79] = "Drekker Del P1 Precision Rifle",
        [80] = "Shelling Arid 5 High Caliber Pistol",
        [81] = "Bataldo Custom K330 Slug Shotgun",
    };

    public static readonly (string FieldKey, string DisplayName, string Description)[] WeaponCoreStatDefs =
    {
        ("Damage", "Damage Multiplier", "Multiplies this weapon's base damage per hit."),
        ("StaggerDamageMulti", "Stagger Multiplier", "Multiplies stagger force dealt per hit."),
        ("PrecisionDamageMulti", "Precision Damage Multiplier", "Multiplies the weak-point/headshot damage bonus."),
        ("DefaultClipSize", "Clip Size Multiplier", "Multiplies magazine/clip size."),
        ("DefaultReloadTime", "Reload Time Multiplier", "Multiplies time to reload."),
    };
}
