namespace BreadMaking.App.Content;

public record BreadEntry(
    string Name,
    string Family,
    string Flour,
    string Hydration,
    string Method,
    string Notes
);

public record BreadCulture(
    string Id,
    string Name,
    string Icon,
    string Overview,
    BreadEntry[] Breads
);

public static class WorldBreadsCatalogue
{
    public static readonly BreadCulture[] Cultures =
    [
        new(
            Id: "france",
            Name: "France",
            Icon: "🥖",
            Overview: "No country has woven bread so tightly into its law, daily ritual and identity as France. The 1993 Décret Pain restricts the \"baguette de tradition\" to four ingredients — flour, water, salt and leaven — and in 2022 UNESCO inscribed the artisanal baguette on its Intangible Cultural Heritage list. French flour is graded by ash content (the \"T\" system): T45 for pastry, T55 for baguettes, T65 for country breads.",
            Breads:
            [
                new("Baguette de tradition", "Lean", "T65", "68–75%", "Poolish or levain", "250 g baton; gentle mix to preserve carotenoids; scored with 5–7 diagonal grignes; baked 240–260 °C with steam. Poolish ferments 12–16 h for an open, nutty crumb."),
                new("Pain de campagne", "Lean", "T65 + rye blend", "72–78%", "Levain", "Country boule or miche. 5–20% rye or wholemeal for depth and crust colour. Cold retard overnight for flavour complexity."),
                new("Ficelle", "Lean", "T65", "68–75%", "As baguette", "A thin baguette at roughly half the dough weight — far more crust per crumb. Often seeded. Same dough, higher surface-area bake."),
                new("Épi de blé", "Lean / Shaped", "T65", "68–75%", "Scissor-cut baguette", "Proofed baton cut with scissors at 45°; segments folded alternately left and right to form a wheat-stalk. Sections pull apart as rolls. Maximises crust."),
                new("Fougasse", "Lean / Shaped", "T65", "70–80%", "Levain or yeast", "Olive-oil enriched; leaf-shaped with cuts that open during baking to produce crisp, fenestrated crust. A Provençal speciality often filled with olives or lardon."),
                new("Croissant", "Viennoiserie", "T45", "35–40%", "Laminated (27 layers)", "Détrempe + 27% butter in 27 alternate layers. Three double folds. Proofed 2–3 h until visibly honeycomb and wobbling. Baked 165–180 °C. A Franco-Austrian hybrid: Austrian Kipferl form, French lamination."),
                new("Pain au chocolat / chocolatine", "Viennoiserie", "T45", "35–40%", "Laminated", "The same croissant dough rolled rectangular around two batons of dark chocolate. A famous regional naming dispute: pain au chocolat in most of France, chocolatine in the south-west."),
                new("Brioche", "Enriched", "T45", "55–65%", "Enriched, 15–20% butter + eggs", "Cream-soft crumb from high butter and egg content. Brioche à tête (the head-topped ball), nanterre (tin with two rows of balls) and feuilletée (laminated). Baked 175–185 °C."),
                new("Pain de mie", "Enriched", "T55", "60%", "Enriched sandwich loaf", "Soft crust baked in a lidded Pullman tin; fine, even crumb from fat and milk. The French sandwich loaf. Similar to shokupan in approach, without the Tangzhong roux."),
                new("Kouign-Amann", "Enriched / Laminated", "T55", "65%", "Caramelised", "Breton speciality: a simple yeast dough with sugar and butter folded in and baked until the bottom caramelises. Crisp outer shell, almost fudge-like interior."),
            ]
        ),

        new(
            Id: "germany",
            Name: "Germany",
            Icon: "🫓",
            Overview: "Germany is the country of rye and of sheer variety. The Deutsches Brotinstitut's Brotregister listed 3,023 registered bread specialities in 2024, and German Brotkultur appears on the national intangible-heritage list. The German Type number is ash content in mg per 100 g (Type 405 ≈ T45; Type 1050 ≈ T80). Rye demands sourdough: its pentosans interfere with gluten and its amylase activity is high — only an acidified dough (pH 4.0–4.3) suppresses the enzymes enough for a stable crumb.",
            Breads:
            [
                new("Vollkornbrot", "Rye", "Roggenvollkornmehl (whole rye)", "90–100%", "Detmold 3-phase sour", "Dense, moist, slice-worthy. A 2–3-day Detmold process builds the sour in three temperature stages. Long bake at 220–240 °C then 200 °C. pH 4.0–4.2 critical for starch stability."),
                new("Pumpernickel", "Rye", "100% whole rye + cracked grain", "100%+", "Multi-day low bake", "Westphalian speciality. Soaked cracked rye, shaped in a Pullman tin and baked 16–24 hours at 100–120 °C (steam oven). Caramelisation colours the crumb near-black; sweetness comes from the Maillard reaction, not added sugar."),
                new("Mischbrot / Graubrot", "Mixed rye–wheat", "~50% rye / 50% wheat", "70–80%", "Rye sourdough + optional yeast", "Germany's commonest daily bread. The rye brings flavour and fibre; the wheat keeps the crumb open. Mild acidity — the Maillard reaction gives a medium-brown crust."),
                new("Roggenmischbrot", "Rye-dominant", ">50% rye", "80–90%", "Rye sourdough", "All rye breads from 51% up to the pure Vollkornbrot family. Flavour and density scale with rye percentage. Requires tin baking above ~70% rye."),
                new("Weizenmischbrot", "Wheat-dominant", ">50% wheat", "65–75%", "Yeast + small rye sour", "Lighter loaves closer to French pain de campagne; the rye fraction (typically 10–30%) adds flavour and shelf life without rye's full sourness."),
                new("Brötchen / Semmel / Kaiser", "Rolls", "Type 550 or 812 wheat", "62–68%", "Yeast, sometimes poolish", "The German bread roll family. Kaiser or Kaisersemmel: five-pointed pinwheel star, crisp shell, soft crumb. Brötchen: the generic term across the north. Schrippe: Berlin's flat oval roll."),
                new("Laugengebäck (Pretzel / Laugenbrötchen)", "Lye bread", "Type 550 wheat", "55–60%", "Stiff yeast dough + lye dip", "The key is the alkaline bath (1–4% NaOH or baked soda, 30–40 °C) before baking: lye gelatinises the surface starch and gives the mahogany crust, distinct snap and characteristic flavour. Baked 200–220 °C without steam."),
                new("Stollen", "Festive", "Type 550 wheat", "~50%", "Enriched yeast dough", "Dresden Christmas loaf. Candied peel, raisins, marzipan. Very low hydration; high butter and sugar. Brushed with melted butter and dusted with icing sugar while still hot. Stores for weeks."),
            ]
        ),

        new(
            Id: "italy",
            Name: "Italy",
            Icon: "🍕",
            Overview: "Italy has no single bread tradition but hundreds — nearly every region and many a town has its own. Two forces organise the variety: grain geography (soft wheat in the north and centre; durum in the south) and the DOP/IGP protected-designation system. Italian flour grades from tipo 00 (highly refined) to integrale (wholemeal); strength is read separately through the W index (forza) and P/L ratio. Durum is milled to semola and semola rimacinata — amber-gold, sweet and nutty.",
            Breads:
            [
                new("Ciabatta", "Lean / Wet", "Tipo 0 or 00, 12–13% protein", "75–85%", "Biga 16–18 h + final dough", "Invented in 1982 by Arnaldo Cavallari as an Italian answer to the baguette. Named for its flat slipper shape. Biga brings flavour and extensibility; very high hydration opens the crumb into irregular, glossy cavities. Handle wet; fold, never knead; bake hot with steam."),
                new("Pane di Altamura DOP", "Durum", "100% semola rimacinata (Murgia durum)", "65–70%", "Lievito madre (natural leaven)", "First bread in the EU to win PDO status (2003). Dense, golden-yellow crumb; thick caramelised crust; nutty, slightly tangy. Shaped as a cappello di prete or a skullcap. Re-risen twice at 30 °C, baked at 230–250 °C."),
                new("Focaccia (Genovese)", "Olive-oil enriched", "Tipo 0", "80–100%", "Yeast or biga", "Ligurian focaccia is an even sheet of dough dimpled with fingertips and topped with olive oil and coarse salt. Baked 220–240 °C. Internal structure more like a high-hydration pizza bianca than a lean loaf."),
                new("Grissini", "Breadsticks", "Tipo 0 or 00", "55–60%", "Yeast, rolled thin", "Turin's thin breadsticks. Stretched by hand or machine to 30–40 cm. Low hydration, minimal fat; baked 200 °C until fully dry and brittle. Wrapped individually in restaurants across Italy. Stirati (hand-pulled) are irregular and more open-textured than machine versions."),
                new("Pane Toscano", "Lean / Saltless", "Tipo 0 or 1", "60–65%", "Natural leaven", "Tuscany's salt-free loaf — a centuries-old tradition linked to the high medieval salt-tax on the Via Francigena. The absence of salt and the levain give a dense, slightly sour crumb. Hardened crust pairs with strongly salted local cured meats."),
                new("Panettone", "Enriched / Festive", "Tipo 00 (W360–400)", "42–48%", "Lievito madre, 3-day process", "The Milanese Christmas dome. Legal minima: 16% butter and 20% candied fruit or raisins. A 3-day process (lievito madre refresh → biga-like first dough → second dough with butter and fruit). Cooled upside-down on skewers to prevent collapse."),
                new("Pizza Bianca Romana", "Flatbread", "Tipo 0 or 00", "80–90%", "Yeast or biga", "Rome's street flatbread — extraordinarily wet, oiled and blistered. Stretched on an oiled baking sheet; dimpled not rolled. The interior stays moist and irregular; the surface blisters at 280–320 °C. Split and filled like a sandwich."),
                new("Piadina Romagnola IGP", "Flatbread", "Tipo 0", "50–55%", "Unleavened or baking soda", "Emilia-Romagna's griddle flatbread. Lard (traditional) or olive oil; rolled thin; cooked on a terracotta testo or cast iron until spotted. Eaten immediately, folded around squacquerone, prosciutto or rucola."),
            ]
        ),

        new(
            Id: "history",
            Name: "Historical Cultures",
            Icon: "🏺",
            Overview: "The breads of France, Germany and Italy are chapters in a much longer story — one that runs from the first leavened loaves of Pharaonic Egypt through Greece and Rome, medieval Europe, the Viennese steam oven, the Jewish diaspora, the northern rye world, and the entirely independent paths taken by Mesoamerican maize and Ethiopian teff. These ten lineages form a web: one hearth in Egypt seeds most leavened wheat bread; a separate flatbread line spreads from the Levant; the northern rye branch and the migrating Jewish tradition stitch regions together; Britain's Chorleywood Process closes the industrial chapter.",
            Breads:
            [
                new("Ancient Egyptian leavened loaf", "Fertile Crescent / Egypt", "Emmer wheat (Triticum dicoccum)", "~60%", "Wild-yeast levain, clay oven", "Pharaonic bread (~3000 BC onwards) was leavened with wild yeast captured in stored grain. Baked in clay cones or pots; emmer gave a dense, slightly sour loaf. Egypt's professional bakers were among the world's first — a reliefs at Giza show industrial-scale bakeries feeding the pyramid builders."),
                new("Greek / Roman wheaten bread", "Classical Antiquity", "Soft wheat (Triticum aestivum)", "55–65%", "Yeast from wine lees or levain", "The Romans professionalised baking: the Pistrina (bakeries with rotary quern-stone mills powered by donkeys) fed Rome's one million inhabitants. A 100 AD Pompeii bakery loaf — the Panis Quadratus — was recovered carbonised, divided into 8 segments; identical loaves are still sold in Naples today."),
                new("Viennese Kaisersemmel & steam oven", "Vienna", "High-gluten wheat", "62–68%", "Yeast, malt syrup", "August Zang opened the first Boulangerie Viennoise in Paris in 1838–1840, bringing the Austrian steam-injection deck oven — the technology that gives the baguette its crisp, glossy crust. Vienna also exported the crescent roll (Kipferl), which French bakers transformed into the laminated croissant."),
                new("Challah", "Jewish Diaspora", "White wheat", "55–65%", "Enriched yeast, eggs + oil (pareve)", "The rich braided Shabbat bread. Braiding began in 15th-century Austria, Jewish bakers adopting local Sunday loaves. Two loaves honour the double manna portion; round loaves at Rosh Hashanah symbolise continuity. Pareve (no dairy), so butter is replaced by oil."),
                new("Bagel", "Jewish Diaspora", "High-gluten wheat", "50–55%", "Yeast, kettle-boiled then baked", "First recorded in Kraków guild records (1610). Poached 60–90 seconds in lye or malt water before baking — the gelatinised surface creates the distinctive chewy shell and dark colour. The hole historically allowed bread to be strung on a dowel for sale."),
                new("Scandinavian crispbread (Knäckebröd)", "Northern Rye World", "Rye wholemeal", "~300%", "Rye sourdough or yeast", "Swedish and Finnish crispbread, baked paper-thin at high heat and then dried to under 5% moisture. The central hole historically let the rounds hang on a pole above the fire to stay dry through winter. Dense in minerals; shelf-stable for months."),
                new("Pita / Khubz", "Middle East", "Wheat (AP or wholemeal)", "65–70%", "Yeast, high-heat bake", "The Levantine pocket bread. Rolled thin and baked at 260–290 °C on a stone or direct flame; steam puffs the loaf into a hollow balloon in 90 seconds. The pocket is a culinary accident of physics, not a design choice: the two surfaces separate before the crust sets."),
                new("Naan", "South Asia", "Wheat (all-purpose)", "65–75%", "Yeast + yogurt, tandoor", "Leavened with yeast and sometimes yogurt for acidity. Slapped against the inside wall of a 480–500 °C tandoor clay oven; gravity stretches it and the fierce heat blisters the surface in 90 seconds. Peshwari, garlic and Kashmiri variants are later restaurant adaptations."),
                new("Nixtamalized tortilla", "Mesoamerica", "Maize (dent corn)", "~120%", "Nixtamal, ground masa, griddle", "The ancient Mesoamerican process of cooking dried maize in an alkaline solution (lime water, cal) — nixtamalization — frees niacin, improves protein quality and gives masa its distinctive flavour. Ground to a fresh dough and pressed thin; cooked 30–45 s per side on a comal or griddle. Arguably the most nutritionally significant culinary invention in the Americas."),
                new("Injera", "Ethiopian / East African", "Teff (Eragrostis tef)", "~300%", "Wild ferment 2–3 days, crêpe griddle", "Ethiopia's national bread and plate in one. Teff flour (and sometimes barley or sorghum) fermented 2–3 days with wild yeast and bacteria; poured thin onto a mitad (clay griddle) and cooked from one side only; the underside bubbles and the surface sets with thousands of pores for catching wot stew. Gluten-free; high in iron and calcium."),
            ]
        ),
    ];
}
