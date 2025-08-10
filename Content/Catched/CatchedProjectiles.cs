using System.Collections.Generic;
using System.IO;
using Gearedup.Content.Items;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Catched
{
    public class CatchedProjectile : ModItem
    {
        public override string Texture => "Gearedup/Content/Placeholder";
        public TypeID catchType;
        public bool notIntended;

        public bool IsNotIntended(Projectile projectile)
        {
            return projectile.ownerHitCheck || projectile.isAPreviewDummy || projectile.minion;
        }
        public void ReloadTypes(Projectile projectile = null)
        {
            Item.SetNameOverride(Lang.GetProjectileName(catchType.id.Value).Value);

            if (projectile != null)
            {
                Item.shoot = projectile.type;
                // reduce damage overtime
                if (GearServerConfig.Get.CatchProjectileDamage)
                {
                    Item.damage = projectile.damage;// / 2;
                    Item.DamageType = projectile.DamageType;
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            catchType.NetSend(writer);
            writer.Write(Item.damage);
            // writer.Write(Item.DamageType);
        }
        public override void NetReceive(BinaryReader reader)
        {
            catchType.NetReceive(reader);
            Item.damage = reader.ReadInt32();
            if (catchType.ValidateAsProjectile())
            {
                ReloadTypes();
            }
        }
        public override void SaveData(TagCompound tag)
        {
            catchType.Save(tag);
            // we need to save this chud

            tag.Add("damage", Item.damage);
            // For some reason this crashes the game
            // for (int i = 0; i < DamageClassLoader.DamageClassCount; i++)
            // {
            //     var dmg = DamageClassLoader.GetDamageClass(i);
            //     tag.Add("saveDamageType", dmg.DisplayName.Value);
            // }
        }
        public override void LoadData(TagCompound tag)
        {
            catchType.Load(tag);

            // if you change language i guess you fucked up
            Item.damage = tag.GetInt("damage");
            // string text = tag.GetString("saveDamageType");
            // for (int i = 0; i < DamageClassLoader.DamageClassCount; i++)
            // {
            //     var dmg = DamageClassLoader.GetDamageClass(i);
            //     if (dmg.DisplayName.Value == text)
            //     {
            //         Item.DamageType = dmg;
            //     }
            // }

            if (catchType.ValidateAsProjectile())
            {
                ReloadTypes();
            }
        }

        // Stacking
        public override bool CanStack(Item source)
        {
            if (source.ModItem != null && source.ModItem is CatchedProjectile target)
            {
                if (target.catchType.id == catchType.id)
                {
                    return true;
                }
            }
            return false;
        }

        public override void SetDefaults()
        {
            Item.damage = 1;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.maxStack = 100;
            Item.consumable = true;
            Item.width = 12;
            Item.height = 12;
            Item.noUseGraphic = true;
            // Item.shoot = proj;
            Item.shootSpeed = 11f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (notIntended)
            {
                tooltips.Add(new TooltipLine(Mod, "Intented", "Might not be intended to catch") { OverrideColor = Color.LightYellow });
            }
            else
            {
                if (Item.stack == Item.maxStack)
                {
                    tooltips.Add(new TooltipLine(Mod, "Infinite", "Infinite, will not be consumed") { OverrideColor = Main.DiscoColor });
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "Finite", "Upon getting 100 stack, will become infinite") { OverrideColor = Color.HotPink });
                }
            }

            if (catchType.id == null || catchType.id.Value <= 0)
            {
                tooltips.Add(new TooltipLine(Mod, "Penis", $"This item didnt load properly gng 💔 \n item id : {catchType.mod} / {catchType.name}") { OverrideColor = Color.Red });
            }

            // if (catchType.unloaded)
            // {
            // 	tooltips.Add(new TooltipLine(Mod,"Unloaded","This item didnt loaded properly \n"+$"ID : ({catchType.Id})   [{catchType._mod} : {catchType._name}] ") {OverrideColor = Color.Red});
            // }
        }

        public override bool ConsumeItem(Player player)
        {
            if (Item.stack == Item.maxStack)
            {
                return false;
            }
            return base.ConsumeItem(player);
        }

        public override void UpdateInventory(Player player)
        {
            if (catchType.id is int validID)
            {
                Item.shoot = validID;
                if (GearServerConfig.Get.CatchProjectileAmmo &&
                player.HeldItem != null &&
                player.HeldItem.type != ModContent.ItemType<DeveloGun>()
                && player.HeldItem.useAmmo > 0)
                {
                    Item.ammo = player.HeldItem.useAmmo;
                }

                if (Item.maxStack == Item.stack)
                {
                    Item.consumable = false;
                }
                else
                {
                    Item.consumable = true;
                }
            }

        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            projectile.hostile = false;
            projectile.friendly = true;
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {

            if (catchType.id is int validID)
            {
                // Check texture
                Main.instance.LoadProjectile(validID);
                Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[validID].Value;
                if (texture == null) return true;

                int frameCount = Main.projFrames[validID];

                Helpme.DrawInventory(spriteBatch, Item.Center - Main.screenPosition, lightColor, texture, frameCount);
                return false;
            }

            // Helpme.DrawInvalid(spriteBatch, Item.Center - Main.screenPosition, rotation);
            return true;
        }
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Check id
            if (catchType.id is int validID)
            {
                // Check texture
                Main.instance.LoadProjectile(validID);
                var proj = ContentSamples.ProjectilesByType[validID];
                if (proj.ModProjectile != null)
                {

                    var lightColor = Color.White;
                    float origScale = proj.scale;
                    Vector2 origPos = proj.Center;
                    proj.scale = 0.4f;
                    proj.Center = position + Main.screenPosition;
                    if (proj.ModProjectile.PreDraw(ref lightColor))
                    {
                        if (!DrawNormally(validID, spriteBatch, position, drawColor))
                        {
                            proj.ModProjectile.PostDraw(lightColor);
                            proj.scale = origScale;
                            proj.Center = origPos;
                        }
                        else
                        {
                            proj.scale = origScale;
                            proj.Center = origPos;
                            return false;
                        }
                    }
                    proj.scale = origScale;
                    proj.Center = origPos;
                }
                else
                {
                    return DrawNormally(validID, spriteBatch, position, drawColor);
                }
                return false;
            }

            // Helpme.DrawInvalid(spriteBatch, position, 0f);

            return true;
        }

        bool DrawNormally(int validID,SpriteBatch spriteBatch,Vector2 position,Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[validID].Value;
            if (texture == null) return true;
            int frameCount = Main.projFrames[validID];
            Helpme.DrawInventory(spriteBatch, position, drawColor, texture, frameCount);
            return false;
        }
    }

    // public class EndlessCatchedProjectile : ModItem
    // {
    //     public override string Texture => "Gearedup/Content/Endless/AmmoPack";
    //     public TypeID catchType;
    //     public bool notIntended;

    //     public bool IsNotIntended(Projectile projectile)
    //     {
    //         return projectile.ownerHitCheck || projectile.isAPreviewDummy || projectile.minion;
    //     }
    //     public void ReloadTypes(Projectile projectile = null)
    //     {
    //         Item.SetNameOverride(Lang.GetProjectileName(catchType.id.Value).Value);
    //         if (projectile != null)
    //         {
    //             Item.shoot = projectile.type;
    //             // reduce damage overtime
    //             Item.damage = projectile.damage;// / 2;
    //             Item.DamageType = projectile.DamageType;
    //         }
    //     }
    //     public override void NetSend(BinaryWriter writer)
    //     {
    //         catchType.NetSend(writer);
    //         writer.Write(Item.damage);
    //         // writer.Write(Item.DamageType);
    //     }
    //     public override void NetReceive(BinaryReader reader)
    //     {
    //         catchType.NetReceive(reader);
    //         Item.damage = reader.ReadInt32();
    //         if (catchType.ValidateAsProjectile())
    //         {
    //             ReloadTypes();
    //         }
    //     }
    //     public override void SaveData(TagCompound tag)
    //     {
    //         catchType.Save(tag);
    //         // we need to save this chud

    //         tag.Add("damage", Item.damage);
    //         // For some reason this crashes the game
    //         // for (int i = 0; i < DamageClassLoader.DamageClassCount; i++)
    //         // {
    //         //     var dmg = DamageClassLoader.GetDamageClass(i);
    //         //     tag.Add("saveDamageType", dmg.DisplayName.Value);
    //         // }
    //     }
    //     public override void LoadData(TagCompound tag)
    //     {
    //         catchType.Load(tag);

    //         // if you change language i guess you fucked up
    //         Item.damage = tag.GetInt("damage");
    //         // string text = tag.GetString("saveDamageType");
    //         // for (int i = 0; i < DamageClassLoader.DamageClassCount; i++)
    //         // {
    //         //     var dmg = DamageClassLoader.GetDamageClass(i);
    //         //     if (dmg.DisplayName.Value == text)
    //         //     {
    //         //         Item.DamageType = dmg;
    //         //     }
    //         // }

    //         if (catchType.ValidateAsProjectile())
    //         {
    //             ReloadTypes();
    //         }
    //     }

    //     // Stacking
    //     public override bool CanStack(Item source)
    //     {
    //         if (source.ModItem != null && source.ModItem is CatchedProjectile target)
    //         {
    //             if (target.catchType.id == catchType.id)
    //             {
    //                 return true;
    //             }
    //         }
    //         return false;
    //     }

    //     public override void SetDefaults()
    //     {
    //         Item.damage = 1;
    //         Item.useStyle = ItemUseStyleID.Swing;
    //         Item.autoReuse = true;
    //         Item.useTurn = true;
    //         Item.useAnimation = 15;
    //         Item.useTime = 15;
    //         Item.maxStack = 1;
    //         Item.consumable = false;
    //         Item.width = 12;
    //         Item.height = 12;
    //         Item.noUseGraphic = true;
    //         // Item.shoot = proj;
    //         Item.shootSpeed = 11f;
    //     }

    //     public override void ModifyTooltips(List<TooltipLine> tooltips)
    //     {
    //         if (notIntended)
    //         {
    //             tooltips.Add(new TooltipLine(Mod, "Intented", "Might not be intended to catch") { OverrideColor = Color.LightYellow });
    //         }

    //         // if (catchType.unloaded)
    //         // {
    //         // 	tooltips.Add(new TooltipLine(Mod,"Unloaded","This item didnt loaded properly \n"+$"ID : ({catchType.Id})   [{catchType._mod} : {catchType._name}] ") {OverrideColor = Color.Red});
    //         // }
    //     }

    //     public override void UpdateInventory(Player player)
    //     {
    //         if (catchType.id is int validID)
    //         {
    //             Item.shoot = validID;
    //             if (GearServerConfig.Get.CatchProjectileAmmo &&
    //             player.HeldItem != null &&
    //             player.HeldItem.type != ModContent.ItemType<DeveloGun>()
    //             && player.HeldItem.useAmmo > 0)
    //             {
    //                 Item.ammo = player.HeldItem.useAmmo;
    //             }
    //         }

    //     }

    //     public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    //     {
    //         var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
    //         projectile.hostile = false;
    //         projectile.friendly = true;
    //         return false;
    //     }

    //     public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    //     {

    //         if (catchType.id is int validID)
    //         {
    //             // Check texture
    //             Main.instance.LoadProjectile(validID);
    //             Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[validID].Value;
    //             if (texture == null) return;

    //             int frameCount = Main.projFrames[validID];

    //             Helpme.DrawInventory(spriteBatch, Item.Center - Main.screenPosition, lightColor, texture, frameCount);
    //         }
    //     }

    //     public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    //     {
    //         // Check id
    //         if (catchType.id is int validID)
    //         {
    //             // Check texture
    //             Main.instance.LoadProjectile(validID);
    //             Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[validID].Value;
    //             if (texture == null) return;

    //             int frameCount = Main.projFrames[validID];

    //             Helpme.DrawInventory(spriteBatch, position, drawColor, texture, frameCount);
    //         }
    //     }
    // }

}