using CatalogService.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Data
{
    public static class CatalogDbContextSeed
    {
        public static async Task SeedAsync(CatalogDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Workstations & Seating" },
                    new Category { Name = "Peripherals" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                var cat1 = context.Categories.First(c => c.Name == "Workstations & Seating").CategoryId;
                var cat2 = context.Categories.First(c => c.Name == "Peripherals").CategoryId;

                var products = new List<Product>
                {
                    // Workstations & Seating
                    new Product { CategoryId = cat1, SKU = "CHAIR-AER-1", Name = "AeroMesh Executive Chair", Brand = "Obsidian", Price = 899.99m, StockQuantity = 25, WeightKg = 22.5m, DimensionsCm = "68x72x120", Material = "Elastomeric Mesh/Aluminum", Color = "Carbon Black", WarrantyPeriod = "12 Years", Manufacturer = "Obsidian Ergonomics", Highlights = "Dynamic lumbar support, 4D armrests, Tilt lock", HardwareInterface = "N/A", PublishStatus = "Published" },
                    new Product { CategoryId = cat1, SKU = "DESK-MTR-1", Name = "Motorized Standing Desk Pro", Brand = "Obsidian", Price = 649.99m, StockQuantity = 15, WeightKg = 45.0m, DimensionsCm = "160x80x70-120", Material = "Solid Walnut/Steel", Color = "Dark Walnut/Matte Black", WarrantyPeriod = "5 Years", Manufacturer = "Obsidian Furniture", Highlights = "Dual motors, Memory presets, Anti-collision", HardwareInterface = "AC Power", PublishStatus = "Published" },
                    new Product { CategoryId = cat1, SKU = "DESK-PAD-1", Name = "Executive Leather Desk Pad", Brand = "Obsidian", Price = 79.99m, StockQuantity = 150, WeightKg = 0.8m, DimensionsCm = "90x40x0.4", Material = "Full-grain Leather", Color = "Midnight Black", WarrantyPeriod = "Lifetime", Manufacturer = "Obsidian Accessories", Highlights = "Water-resistant, Anti-slip base, Soft edges", HardwareInterface = "N/A", PublishStatus = "Published" },
                    new Product { CategoryId = cat1, SKU = "MNTR-ARM-2", Name = "Dual Monitor Articulating Arm", Brand = "Obsidian", Price = 149.99m, StockQuantity = 50, WeightKg = 4.2m, DimensionsCm = "110x12x45", Material = "Aircraft-grade Aluminum", Color = "Gunmetal", WarrantyPeriod = "10 Years", Manufacturer = "Obsidian Mounts", Highlights = "Gas spring, Cable management, VESA compatible", HardwareInterface = "VESA 75/100", PublishStatus = "Published" },
                    new Product { CategoryId = cat1, SKU = "LITE-BAR-1", Name = "Monitor Light Bar Halo", Brand = "Obsidian", Price = 129.99m, StockQuantity = 80, WeightKg = 0.6m, DimensionsCm = "45x9x9", Material = "Aluminum/Polycarbonate", Color = "Space Gray", WarrantyPeriod = "2 Years", Manufacturer = "Obsidian Illumination", Highlights = "Auto-dimming, Glare-free, 3000K-6000K", HardwareInterface = "USB-C", PublishStatus = "Published" },
                    new Product { CategoryId = cat1, SKU = "CHAIR-LUM-2", Name = "Lumbar Support Cloud Cushion", Brand = "Obsidian", Price = 49.99m, StockQuantity = 120, WeightKg = 0.4m, DimensionsCm = "40x35x10", Material = "High-density Memory Foam", Color = "Charcoal", WarrantyPeriod = "1 Year", Manufacturer = "Obsidian Ergonomics", Highlights = "Breathable mesh cover, Ergonomic contour", HardwareInterface = "N/A", PublishStatus = "Published" },
                    new Product { CategoryId = cat1, SKU = "DESK-TRAY-1", Name = "Under-Desk Cable Management Tray", Brand = "Obsidian", Price = 34.99m, StockQuantity = 200, WeightKg = 1.2m, DimensionsCm = "60x12x8", Material = "Powder-coated Steel", Color = "Matte Black", WarrantyPeriod = "5 Years", Manufacturer = "Obsidian Furniture", Highlights = "High capacity, Easy installation", HardwareInterface = "N/A", PublishStatus = "Published" },
                    new Product { CategoryId = cat1, SKU = "FOOT-RST-1", Name = "Adjustable Ergonomic Footrest", Brand = "Obsidian", Price = 59.99m, StockQuantity = 60, WeightKg = 2.0m, DimensionsCm = "45x35x15", Material = "ABS Plastic/Rubber", Color = "Obsidian Black", WarrantyPeriod = "3 Years", Manufacturer = "Obsidian Ergonomics", Highlights = "Adjustable angle, Massage texture", HardwareInterface = "N/A", PublishStatus = "Published" },

                    // Peripherals
                    new Product { CategoryId = cat2, SKU = "KB-MECH-75", Name = "Flux Core Alpha 75% Keyboard", Brand = "Obsidian", Price = 189.99m, StockQuantity = 40, WeightKg = 1.4m, DimensionsCm = "32x14x3.5", Material = "CNC Aluminum", Color = "Anodized Black", WarrantyPeriod = "2 Years", Manufacturer = "Obsidian Peripherals", Highlights = "Hot-swappable switches, Gasket mount, QMK/VIA", HardwareInterface = "USB-C/Bluetooth", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "MSE-PRO-UL", Name = "AeroGlide Ultralight Wireless Mouse", Brand = "Obsidian", Price = 119.99m, StockQuantity = 75, WeightKg = 0.06m, DimensionsCm = "12x6x3.8", Material = "Magnesium Alloy", Color = "Matte Black", WarrantyPeriod = "2 Years", Manufacturer = "Obsidian Peripherals", Highlights = "26K DPI sensor, Optical switches, 4K polling", HardwareInterface = "RF 2.4GHz/USB-C", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "MIC-CST-1", Name = "Studio Cast Condenser Microphone", Brand = "Obsidian", Price = 249.99m, StockQuantity = 20, WeightKg = 0.9m, DimensionsCm = "20x6x6", Material = "Brass/Steel", Color = "Satin Black", WarrantyPeriod = "3 Years", Manufacturer = "Obsidian Audio", Highlights = "Cardioid polar pattern, Built-in pop filter, Low self-noise", HardwareInterface = "XLR", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "CDBRD-1", Name = "MacroPad Pro Control Deck", Brand = "Obsidian", Price = 149.99m, StockQuantity = 45, WeightKg = 0.5m, DimensionsCm = "15x10x3", Material = "Aluminum/Glass", Color = "Dark Gray", WarrantyPeriod = "2 Years", Manufacturer = "Obsidian Peripherals", Highlights = "LCD keys, Rotary encoders, App profiles", HardwareInterface = "USB-C", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "HDST-NCT-1", Name = "Aura Sound Shell ANC Headset", Brand = "Obsidian", Price = 299.99m, StockQuantity = 30, WeightKg = 0.35m, DimensionsCm = "20x18x8", Material = "Titanium/Protein Leather", Color = "Abyss Black", WarrantyPeriod = "2 Years", Manufacturer = "Obsidian Audio", Highlights = "Hybrid ANC, Spatial audio, 40hr battery", HardwareInterface = "Bluetooth 5.3/3.5mm", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "WEBCAM-4K", Name = "Obsidian Chrono 4K Webcam", Brand = "Obsidian", Price = 199.99m, StockQuantity = 60, WeightKg = 0.2m, DimensionsCm = "8x4x4", Material = "Aluminum/Glass", Color = "Graphite", WarrantyPeriod = "2 Years", Manufacturer = "Obsidian Optics", Highlights = "Sony Starvis sensor, AI framing, Privacy cover", HardwareInterface = "USB-C", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "DOCK-TB4-1", Name = "Thunderbolt 4 Enterprise Dock", Brand = "Obsidian", Price = 349.99m, StockQuantity = 25, WeightKg = 0.8m, DimensionsCm = "22x8x3", Material = "Machined Aluminum", Color = "Space Gray", WarrantyPeriod = "3 Years", Manufacturer = "Obsidian Tech", Highlights = "Dual 4K at 60Hz, 96W PD, 8 ports", HardwareInterface = "Thunderbolt 4", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "SPKR-MON-1", Name = "Reference Studio Monitors (Pair)", Brand = "Obsidian", Price = 399.99m, StockQuantity = 15, WeightKg = 8.5m, DimensionsCm = "25x16x20", Material = "Acoustic MDF", Color = "Matte Black/Kevlar", WarrantyPeriod = "3 Years", Manufacturer = "Obsidian Audio", Highlights = "Bi-amped, Class D, Kevlar woofers", HardwareInterface = "TRS/XLR/RCA", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "WRIST-REST-1", Name = "Hardwood Ergonomic Wrist Rest", Brand = "Obsidian", Price = 39.99m, StockQuantity = 100, WeightKg = 0.3m, DimensionsCm = "32x8x2", Material = "Solid Walnut", Color = "Natural Dark Wood", WarrantyPeriod = "Lifetime", Manufacturer = "Obsidian Accessories", Highlights = "Smooth finish, Anti-slip rubber feet", HardwareInterface = "N/A", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "CBL-COIL-1", Name = "Premium Coiled Aviator Cable", Brand = "Obsidian", Price = 49.99m, StockQuantity = 150, WeightKg = 0.15m, DimensionsCm = "150x1x1", Material = "Nylon Paracord/Metal", Color = "Carbon", WarrantyPeriod = "1 Year", Manufacturer = "Obsidian Accessories", Highlights = "5-pin aviator connector, Double-sleeved", HardwareInterface = "USB-C to USB-A", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "DP-CBL-8K", Name = "DisplayPort 2.1 8K Cable", Brand = "Obsidian", Price = 24.99m, StockQuantity = 200, WeightKg = 0.1m, DimensionsCm = "200x0.5x0.5", Material = "Braided Nylon/Gold", Color = "Black", WarrantyPeriod = "Lifetime", Manufacturer = "Obsidian Tech", Highlights = "80Gbps bandwidth, VESA Certified", HardwareInterface = "DisplayPort", PublishStatus = "Published" },
                    new Product { CategoryId = cat2, SKU = "FLSDV-2TB", Name = "NVMe Portable SSD 2TB", Brand = "Obsidian", Price = 229.99m, StockQuantity = 35, WeightKg = 0.08m, DimensionsCm = "10x4x1", Material = "Aluminum Frame", Color = "Gunmetal", WarrantyPeriod = "5 Years", Manufacturer = "Obsidian Storage", Highlights = "Up to 2000MB/s, IP65 rated, Hardware encryption", HardwareInterface = "USB-C 3.2 Gen 2x2", PublishStatus = "Published" }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
