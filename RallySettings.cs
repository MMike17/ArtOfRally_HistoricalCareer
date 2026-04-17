using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using static AreaManager;
using static Car;
using static ConditionTypes;

namespace HistoricalCareer
{
    public class RallySettings
    {
        public CarClass carClass;
        public string areaName;
        public string rallyName;
        public string pilotName;
        public Sprite pilotPicture;
        public int pilotPictureYear;
        public int carIndex;
        public int liveryIndex;
        public int locationPictureIndex;
        public string loreText;

        public Livery livery;
        public Sprite locationPicture;
        public Season season;
        public bool needsDLC;

        public RallySettings(
            CarClass carClass,
            int year,
            Areas area,
            string areaName,
            string rallyName,
            string pilotName,
            Sprite pilotPicture,
            int pilotPictureYear,
            int carIndex,
            int liveryIndex,
            int locationPictureIndex,
            int[] stagesIndeces,
            Weather[] weathers,
            int restarts,
            string loreText
        )
        {
            this.carClass = carClass;
            this.areaName = areaName;
            this.rallyName = rallyName;
            this.pilotName = pilotName;
            this.pilotPicture = pilotPicture;
            this.pilotPictureYear = pilotPictureYear;
            this.carIndex = carIndex;
            this.liveryIndex = liveryIndex;
            this.loreText = loreText;

            livery = GetCarLiveries(CarManager.GetCurrentCarsListForClass(carClass)[carIndex].prefabName)[liveryIndex];

            string stagePrefix = AreaManager.AreaDictionary[area].stageList[locationPictureIndex].LeaderboardStagePrefixString;
            string locationPicturePath = "Sprites/TrackBackgrounds/" + area.ToString().ToLower() + "_" + stagePrefix[stagePrefix.Length - 1];
            locationPicture = Resources.Load<Sprite>(locationPicturePath);

            // check stages
            List<int> correctedStages = new List<int>();
            int maxIndex = AreaManager.AreaDictionary[area].stageList.Count;

            foreach (int index in stagesIndeces)
            {
                if (index >= maxIndex)
                    Main.Error("Stage index " + index + " is out of bounds (" + maxIndex + "), the stage will be ignored.");
                else
                    correctedStages.Add(index);
            }

            stagesIndeces = correctedStages.ToArray();

            // check weathers
            List<ConditionTypes.Weather> areaWeathers = AreaManager.GetWeatherForCurrentArea(area);
            List<ConditionTypes.Weather> correctedWeathers = new List<ConditionTypes.Weather>();

            foreach (ConditionTypes.Weather weather in weathers)
            {
                if (!areaWeathers.Contains(weather))
                    Main.Error("Weather " + weather + " is invalid for this area (" + area + "), the weather will be ignored.");
                else
                    correctedWeathers.Add(weather);
            }

            weathers = correctedWeathers.ToArray();

            // setup season
            season = new Season(
                year,
                carClass,
                1,
                Main.settings.shortRallies ? 1 : stagesIndeces.Length,
                restarts,
                "UNLOCKABLE_" + year + "_BONUS",
                true,
                AIDriverSkillTables.AI_Skill.EASY
            );

            // setup rally
            season.Rallies.Add(new RallyData());
            season.Rallies[0].SetArea((int)area);
            season.Rallies[0].SetStageCount(Main.settings.shortRallies ? 1 : stagesIndeces.Length);

            for (int i = 0; i < (Main.settings.shortRallies ? 1 : stagesIndeces.Length); i++)
            {
                int index = stagesIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, area);
                season.Rallies[0].SetStage(i, stage);
                season.Rallies[0].SetWeatherForStage(i, weathers[i]);
            }

            SaveManager.LoadSeasonData(season);

            // checks for DLC use
            needsDLC = area == AreaManager.Areas.AUSTRALIA;

            if (!needsDLC)
            {
                switch (carClass)
                {
                    case CarClass.GROUP_2:
                        needsDLC = carIndex == 7;
                        break;

                    case CarClass.GROUP_4:
                        needsDLC = carIndex == 10;
                        break;

                    case CarClass.GROUP_B:
                        needsDLC = carIndex == 16;
                        break;

                    case CarClass.GROUP_A:
                        needsDLC = carIndex == 5;
                        break;
                }
            }
        }

        private static List<Livery> GetCarLiveries(string carName)
        {
            List<Livery> liveries = new List<Livery>();

            foreach (string textureName in LiveryManager.GenericTextures)
            {
                liveries.Add(new Livery(
                    textureName,
                    LiveryManager.GetStandardLiveryPath(carName, textureName),
                    Livery.LiveryType.Standard,
                    carName,
                    false
                ));
            }

            return liveries;
        }

        public static void GenerateGroup2Seasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_2;

            RallyManager.AddCustomRally(
                group, 1966, Areas.FINLAND, "Finland", "1000 Lakes rally", "Timo Mäkinen",
                assembly, pilotPicturePath, 1966, 1, 0, 2,
                new int[] { 0, 6 }, new Weather[] { Weather.Morning, Weather.Afternoon }, ComputeRestarts(0 / 6f, 2),
                "Following their 1965 win, <b>Timo Mäkinen</b> and his copilot <b>Pekka Keskitalo</b> took advantage of their Mini's front-wheel-drive and light body to excell on their home country's gravel jumps, becoming the first \"flying finns\" with <b>Simo Lampinen</b> and <b>Rauno Aaltonen</b>, who placed 3rd on the same rally."
            );
            RallyManager.AddCustomRally(
                group, 1967, Areas.SARDINIA, "Italy", "Rally dei Fiori", "Jean-François Piot",
                assembly, pilotPicturePath, 1967, 5, 1, 0,
                new int[] { 2, 6, 0 }, new Weather[] { Weather.Fog, Weather.Afternoon, Weather.Sunset }, ComputeRestarts(1 / 6f, 3),
                "After winning the <b>Tour de Corse</b> and the <b>Coupe de Alpes</b> the previous year, <b>Jean-François Piot</b> joined by copilot <b>Nicolas Roure</b> armed with a prototype Renault 8 Gordini 1440 dominated tarmac-gravel mixed stages, beating its competition in the mediterranean conditions."
            );
            RallyManager.AddCustomRally(
                group, 1968, Areas.GERMANY, "West Germany", "Wiesbaden German Rally", "Pauli Toivonen",
                assembly, pilotPicturePath, 1969, 4, 5, 0, new int[] { 0, 5, 3, 11 },
                new Weather[] { Weather.Rain, Weather.Sunset, Weather.Fog, Weather.Afternoon }, ComputeRestarts(2 / 6f, 4),
                "For his debut in a Porsche, <b>Pauli Toivonen</b> couldn't secure a <b>Monte-carlo</b> win, being overtaken by <b>Vic Elford</b>. But with the help of <b>Martti Kolari</b> as copilot, they managed a win in the <b>West German rally</b>. Pauli continued winning that same year in Austria and Swizerland with different copilots."
            );
            RallyManager.AddCustomRally(
                group, 1969, Areas.SARDINIA, "Italy", "Rally Sanremo", "Harry Källström",
                assembly, pilotPicturePath, 1970, 6, 3, 0, new int[] { 2, 6, 10, 0 },
                new Weather[] { Weather.Sunset, Weather.Afternoon, Weather.Sunset, Weather.Afternoon }, ComputeRestarts(3 / 6f, 4),
                "Nicknamed \"Sputnik\" because of how fast his career took off, driver and later actorn <b>Harry Källström</b> took the wheel of a Lancia Fulvia with his copilot <b>Häggbom Gunnar</b> to win the <b>Sanremo Rally</b> as well as the <b>Spanish RACE Rally</b> and <b>British RAC Rally</b> that year, earning their first European champions title."
            );
            RallyManager.AddCustomRally(
                group, 1970, Areas.FINLAND, "Finland", "1000 Lakes rally", "Hannu Mikkola",
                assembly, pilotPicturePath, 1966, 0, 1, 2, new int[] { 0, 5, 6, 8, 2 },
                new Weather[] { Weather.Morning, Weather.Afternoon, Weather.Morning, Weather.Sunset, Weather.Rain }, ComputeRestarts(4 / 6f, 5),
                "After their win in the grueling <b>London-Mexico World Cup Rally</b>, <b>Hannu Mikkola</b> and the exceptional navigator <b>Gunnar Palm</b> brought their Ford Escort twin cam to Finland. With its engine originally developped for the Lotus Elan and its lights body shell the Escort proved to be a solid contender for the rougher rallies."
            );
            RallyManager.AddCustomRally(
                group, 1970, Areas.GERMANY, "Austria", "Rally Munich-Vienne-Budapest", "Jean-Claude Andruet",
                assembly, pilotPicturePath, 1977, 2, 5, 6, new int[] { 1, 8, 10, 5, 7 },
                new Weather[] { Weather.Rain, Weather.Morning, Weather.Rain, Weather.Sunset, Weather.Fog }, ComputeRestarts(5 / 6f, 5),
                "After dominating in the french rallies, <b>Jean-Claude Andruet</b> wildly swung his <b>Alpine A110</b> with <b>Michèle Veron</b> as copilot in this rally spanning between Germany, Austria and Hungary. The battle with Fords and Porsches was fierce but they won the French and European rally championships titles."
            );
            RallyManager.AddCustomRally(
                group, 1971, Areas.GERMANY, "East Germany", "Pneumant Rally", "Sobiesław Zasada",
                assembly, pilotPicturePath, 1971, 3, 2, 0, new int[] { 7, 0, 9, 1, 4, 8 },
                new Weather[] { Weather.Fog, Weather.Rain, Weather.Rain, Weather.Afternoon, Weather.Fog, Weather.Sunset },
                ComputeRestarts(6 / 6f, 6),
                "Out with the Porsche, in with a <b>BMW 2002 Ti</b> that <b>Sobiesław Zasada</b> hurls through the fog and rain of <b>East Germany</b>, as if it was a javelin from his youth. Pairing up with copilot <b>Adam Wędrychowski</b>, he ended up winning his third european champion in <b>1971</b>, winning both the <b>East Germany</b> and <b>Polish</b> rally."
            );
        }

        public static void GenerateGroup3Seasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_3;

            RallyManager.AddCustomRally(
                group, 1971, Areas.KENYA, "Kenya", "Safari rally", "Edgar Herrmann",
                assembly, pilotPicturePath, 1970, 9, 0, 4, new int[] { 4, 6 },
                new Weather[] { Weather.Sunset, Weather.Rain }, ComputeRestarts(0 / 9f, 2),
                "German born pilot Edgar Hermann shelved his <b>Porsche 911</b> for a <b>Datsun 1600 SSS</b>, landing him a series of wins in <b>1970</b> before winning a second <b>Safari rally</b> behind the wheel of a <b>Datsun 240Z</b>, guided by <b>Hans Schuller</b>. Hard-eyed, confident and said to be trailed by good-looking women, Edgar ended up falling in love with Kenya from which he was eventually naturalized."
            );
            RallyManager.AddCustomRally(
                group, 1974, Areas.SARDINIA, "Italy", "Rally Sanremo", "Sandro Munari",
                assembly, pilotPicturePath, 1975, 3, 0, 0, new int[] { 2, 6, 0 },
                new Weather[] { Weather.Morning, Weather.Rain, Weather.Afternoon }, ComputeRestarts(1 / 9f, 3),
                "Although it was unreliable, the new <b>Lancia Stratos</b> proved to be a perfect match for <b>Sandro Munari</b>, the <i>Dragon of Cavarzere</i>. He secured his first wins with it the year prior and knew that it would bring him and his long time copilot <b>Mario Mannucci</b> a win at the <b>Rally Sanremo</b>, if they could tame that beast. They were closely overtaken by  <b>Timo Mäkinen</b> for best driver that year but still pulled Lancia to the top after <b>Jean-Claude Andruet</b> closed the new WRC championship with a <b>Tour de Corse</b> win."
            );
            RallyManager.AddCustomRally(
                group, 1974, Areas.GERMANY, "Czechoslovakia", "Rally Jeseníky", "Jiří Šedivý",
                assembly, pilotPicturePath, 1974, 7, 0, 4, new[] { 5, 2, 3 },
                new Weather[] { Weather.Sunset, Weather.Morning, Weather.Afternoon }, ComputeRestarts(2 / 9f, 3),
                "In <b>1974</b>, the engineers and pilots <b>Jiří Šedivý</b> and <b>Jiří Janeček</b> were hard at work in the Skoda factory of Kvasiny, cooking up the <b>Skoda 200 RS</b>, a heavily modified <i>Rally Sport</i> prototype. They took their learnings from their work on the <b>Skoda 110 R</b> to give the <b>200 RS</b> incredible power and acceleration as well as new rear suspension geometry. They ripped the asphalt of <b>Czechoslovakia</b> and impressed the FIA so much that they decided to ban that type of heavy modification for the <b>1975</b> WRC season. But <b>Jiří</b> and <b>Jiří</b> were not done with their tinkering and presented the <b>Skoda 130 RS</b> the next year."
            );
            RallyManager.AddCustomRally(
                group, 1975, Areas.GERMANY, "East Germany", "Rally Wartburg", "Błażej Krupa",
                assembly, pilotPicturePath, 1975, 6, 0, 0, new int[] { 9, 4, 1, 8 },
                new Weather[] { Weather.Fog, Weather.Afternoon, Weather.Fog, Weather.Fog }, ComputeRestarts(3 / 9f, 4),
                "The tires of <b>Błażej Krupa</b>'s car taste the asphalt and the dirt again in <b>1974</b> after Renault decided to fund his return to rally for an overall win in <b>Poland</b>. It's with a <b>Renault 17</b> and his copilot <b>Piotr Mystkowski</b> that Błażej went on to win the <b>1975</b> and <b>1976</b> <b>Cup of Peace and Friendship (CoPaF)</b>. In the heavy fog of <b>Rally Wartburg</b> two <b>Renault 17</b> dominated more than 5 minutes ahead of its competition closing the rally before winning again on the ice of <b>Rallye Russkaya Zima</b>."
            );
            RallyManager.AddCustomRally(
                group, 1976, Areas.GERMANY, "East Germany", "Rally Sachsenring", "Miloslav Zapadlo",
                assembly, pilotPicturePath, 1977, 8, 0, 0, new int[] { 0, 9, 7, 1 },
                new Weather[] { Weather.Rain, Weather.Fog, Weather.Sunset, Weather.Fog }, ComputeRestarts(4 / 9f, 4),
                "<b>Miloslav Zapadlo</b>, a hot blooded pilot, took the wheel of the new <b>Skoda 130 RS</b>, the pinacle of Czech engineering based on the <b>Skoda 200 RS</b> beast, with his copilot <b>Jiří Motal</b> to defend the pride of his company and country. They didn't win in Czechoslovakia that year but won in the <b>Rally Sachsenring</b> in <b>East Germany</b> with a large gap. A brief moment of fame during the <b>1977 Monte-Carlo Rally</b> cemented Skoda's new model which went on to dominate 1978 <b>CoPaF</b> and eastern europe rallies of the <b>European Rally Championship</b>."
            );
            RallyManager.AddCustomRally(
                group, 1976, Areas.AUSTRALIA, "West Australia", "Commonwealth Bank Rally", "Frank Johnson",
                assembly, pilotPicturePath, 0, 2, 4, 4, new int[] { 10, 7, 4, 2 },
                new Weather[] { Weather.Rain, Weather.Morning, Weather.Afternoon, Weather.Morning }, ComputeRestarts(5 / 9f, 4),
                "The strong winds and wet weather of <b>Australia</b> left several stages of the <b>Commonwealth Bank Rally</b> out of order. The sun came back and gave <b>Frank Johnson</b> and his copilot <b>Bill Clark</b> a chance at a win with their <b>Mazda RX-3</b> in their second ever rally. They ended up in second position but the <b>RX-3</b> couldn't keep up with Porsches and Datsuns in other Australian rallies. The <b>RX-3</b> left a lasting impression in Australia rally and tourning races driven by teams like the <b>Kabel</b> brothers."
            );
            RallyManager.AddCustomRally(
                group, 1976, Areas.SARDINIA, "Italy", "Rally of the Elba island", "Markku Alén",
                assembly, pilotPicturePath, 1976, 1, 1, 6, new int[] { 9, 7, 2, 6, 8 },
                new Weather[] { Weather.Rain, Weather.Rain, Weather.Morning, Weather.Afternoon, Weather.Sunset }, ComputeRestarts(6 / 9f, 5),
                "Imported straight from <b>Finland</b>, <b>Markku Alén</b> bolted to the brand new engineering marvel from Fiat, came in to teach a lesson to Lancia on their own grounds in the <b>European Rally Championship</b>. Teamed up with <b>Ilkka Kivimäki</b>, they took a while to adapt to the unfamiliar italian roads but ended up chaining first positions. They went on to win the <b>1000 Lakes Rally</b> later that year in their home country, fighting against the new version of the <b>Ford Escort</b>."
            );
            RallyManager.AddCustomRally(
                group, 1976, Areas.FINLAND, "Finland", "Champion Nordic Rally", "Timo Mäkinen",
                assembly, pilotPicturePath, 1978, 0, 5, 0, new int[] { 1, 3, 7, 10, 4 },
                new Weather[] { Weather.Morning, Weather.Sunset, Weather.Night, Weather.Afternoon, Weather.Sunset }, ComputeRestarts(7 / 9f, 5),
                "After 3 consecutive wins in the <b>RAC Rally</b> with the British copilot <b>Henry Liddon</b>, <b>Timo Mäkinen</b> decided to finish his Ford career on a Finish win his fellow countryman <b>Erkki Salonen</b>. The new generation of the <b>Ford Escort</b> proved to be as sturdy as its ancestor during the <b>Total Rally South Africa</b> and rushed the pair to the top of the podium in the <b>Champion Nordic Rally</b>. Timo went on to drive the competitor <b>Fiat 131</b> the next year and switched to <b>Peugeot 504 V6</b> after that."
            );
            RallyManager.AddCustomRally(
                group, 1978, Areas.KENYA, "Kenya", "Safari Rally", "Vic Preston jr",
                assembly, pilotPicturePath, 1978, 5, 0, 4, new int[] { 0, 8, 4, 6, 10, 3 },
                new Weather[] { Weather.Sunset, Weather.Night, Weather.Morning, Weather.Afternoon, Weather.Sunset, Weather.Afternoon },
                ComputeRestarts(8 / 9f, 6),
                "Rally runs in the blood of the Preston family. The father, former local champion, assisted while the team of his son, <b>Vic Preston Jr</b> and his copilot <b>John Lyall</b>, took a heavily modified <b>Porsche 911 SC</b> to the <b>Safari Rally</b>. <b>Ford</b> and <b>Fiat</b> which dominated the rest of the <b>WRC</b> rallies stepped out of the Safari which enabed our local pilots to get a 2nd position. But it's the <b>Lancia 037 Rally</b> that brought Vic to fame with back to back wins in 1985 making him one of the most distinguished Kenyan pilots."
            );
            RallyManager.AddCustomRally(
                group, 1982, Areas.SARDINIA, "Italy", "Targa Florio Rally", "Tonino Tognana",
                assembly, pilotPicturePath, 1982, 4, 4, 10, new int[] { 3, 9, 1, 5, 7, 11 },
                new Weather[] { Weather.Morning, Weather.Afternoon, Weather.Afternoon, Weather.Sunset, Weather.Rain, Weather.Sunset },
                ComputeRestarts(9 / 9f, 6),
                "Back to the roots of automotive racing, in the mythical <b>Targa Florio Rally</b>, an unexpected sight, the renouned F1 team was here with a custom <b>Ferrari 308 GTB</b> prepared by car dealer <b>Micheletto</b> from Padua, whom was sent bear chassis and spare parts to make a true rally machine. The previous year's winner, <b>Jean-Claude Andruet</b>, was thrown out of the first position by <b>Tonino Tognana</b> and his copilot <b>Massimo De Antonio</b>. The pair won the Italian championship while Andruet finished 2nd on the <b>Tour de France</b>, proving the Ferrari was a decently competitive car."
            );
        }

        public static void GenerateGroup4Seasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_4;

            RallyManager.AddCustomRally(
                group, 1975, Areas.SARDINIA, "Italy", "Rallye dell'Isola d'Elba", "Amilcare Balestrieri",
                assembly, pilotPicturePath, 0, 7, 4, 6, new int[] { 9, 7, 2 },
                new Weather[] { Weather.Morning, Weather.Afternoon, Weather.Morning }, ComputeRestarts(0 / 9f, 3),
                "For the <b>1975</b> season <b>Alfa Romeo</b> hired the very successful <b>Lancia</b> pilot <b>Amilcare Balestrieri</b> and his copilot <b>Enrico Gigli</b> to man their <b>Alfa Romeo Alfetta Turbodelta</b>, which proved to be a winning bet as their pilots finished first, second and fourth with <b>Andruet</b> on the <b>Rallye dell'Isola d'Elba</b>. <b>Amilcare</b> finished third in the <b>Italian championship</b> while fighting <b>Fiat</b> and <b>Lancia</b>, and fourth in the <b>ERC</b> that year. The car was often seen in <b>France</b> with <b>Jean-Claude Andruet</b> but also in the <b>Rally Safari</b> with a series of wins in <b>1979</b> piloted by <b>Rob Collinge</b> who scored a second place in the <b>Kenya championship</b> and an astonishing four wins."
            );

            RallyManager.AddCustomRally(
                group, 1977, Areas.SARDINIA, "France", "Ronde de la Giraglia", "Guy Fréquelin",
                assembly, pilotPicturePath, 1981, 6, 1, 2, new int[] { 7, 4, 2, 3 },
                new Weather[] { Weather.Afternoon, Weather.Sunset, Weather.Morning, Weather.Afternoon },
                ComputeRestarts(1 / 9f, 4),
                "The successor to the mythical <b>Alpine A110</b> was waiting patiently in the <b>1976</b> French rallies making it to a few podiums. But it's in <b>1977</b> with <b>Guy Fréquelin</b> and his copilot <b>Jacques Delaval</b> that the <b>Alpine A310</b> really showed the power of its brand new V6. Between <b>Guy Fréquelin</b> and <b>Bernard Béguin</b> the <b>A310</b> won 10 rallies in the <b>French Rally Championship</b> setting a new record. The car never saw use outside of France and it quickly disappeard at the end of <b>1977</b>. But for a short while the <b>A310</b> went toe to toe with the greats like the <b>Porsche 911 Carrera</b>, the <b>Lancia Stratos</b> and <b>Fiat 131 Abarth</b>."
            );

            RallyManager.AddCustomRally(
                group, 1978, Areas.JAPAN, "Japan", "Touge Usui", "Keiichi Tsuchiya",
                assembly, pilotPicturePath, 1984, 5, 5, 10, new int[] { 4, 11, 10, 5 },
                new Weather[] { Weather.Sunset, Weather.Night, Weather.Night, Weather.Fog }, ComputeRestarts(2 / 9f, 4),
                "Little <b>Keiichi Tsuchiya</b> used to listen to the sound of screeching rubber at night while the local street racers, called <b>\"hashiriya\"</b>, were going down the narrow roads of the mountain pass. In <b>1974</b> he bought his first car and started training diligently around his home town of <b>Tōmi</b> before joining circuits three years later with a <b>Datsun Sunny B110</b>. One night he headed to the <b>Usui pass</b> with a new white and black <b>Toyota Sprinter Trueno</b> and challenged the best <b>hashiriya</b> of the region, at the end of the night the pilots called him the <b>\"Mountain King\"</b>. He kept on racing and put up quite a show in <b>1984</b> and <b>1985</b> racing his <b>Trueno</b> on the circuits and introduced everyone to his art of drifting in <b>1987</b> with his drift movie <b>Pulpsy</b>."
            );

            RallyManager.AddCustomRally(
                group, 1979, Areas.KENYA, "Kenya", "Safari rally", "Shekhar Mehta",
                assembly, pilotPicturePath, 1979, 8, 0, 4, new int[] { 0, 8, 4, 6 },
                new Weather[] { Weather.Sunset, Weather.Rain, Weather.Rain, Weather.Afternoon }, ComputeRestarts(3 / 9f, 4),
                "Back again after a hiatus on the <b>Safari Rally</b>, <b>Shekhar Mehta</b> and his <b>Safari</b> copilot <b>Mike Doughty</b> started a series of five <b>Safari</b> wins that would make history. Their long-time partner <b>Datsun</b> provided them with a <b>Datsun 160J</b> that soared forward on the kenyan tracks, battleing for the top position in the <b>Kenyan championship</b> with the <b>Alfa Romeo Alfetta GTV</b>. <b>Shekhar</b> went on to place first on most of the <b>1980</b> events he raced in, switching his <b>Datsun</b> for an <b>Opel</b>."
            );

            RallyManager.AddCustomRally(
                group, 1982, Areas.SARDINIA, "France", "Tour de Corse", "Jean Ragnotti",
                assembly, pilotPicturePath, 1976, 0, 0, 2, new int[] { 7, 4, 2, 3, 9 },
                new Weather[] { Weather.Fog, Weather.Fog, Weather.Afternoon, Weather.Rain, Weather.Sunset },
                ComputeRestarts(4 / 9f, 5),
                "French champion in <b>1980</b>, <b>Jean Ragnotti</b>, nicknamed \"the acrobat\", teamed up with his long time copilot <b>Jean-Marc Andrié</b> to win several french events between two <b>24 hours of Le Mans</b> participations. He introduced the world to the lightning fast <b>Renault 5 Turbo</b> the previous year by winning the <b>Rally Montecarlo</b>. Followed closely by <b>Jean-Claude Andruet</b> in his <b>Ferrari 308 GTB</b>, he took advantage of the poor visibility and wet roads of <b>Corsica</b> that year to fly around curves. <b>Renault</b> was so pleased with his performance that they made a special <b>\"Tour de Corse\"</b> edition for the next rally season which was considered for the new <b>Group B</b> homologation."
            );

            RallyManager.AddCustomRally(
                group, 1982, Areas.GERMANY, "Germany", "Rallye Vorderpfalz", "Klaus Fritzinger",
                assembly, pilotPicturePath, 1982, 3, 5, 10, new int[] { 11, 8, 3, 5, 9 },
                new Weather[] { Weather.Rain, Weather.Afternoon, Weather.Rain, Weather.Morning, Weather.Afternoon },
                ComputeRestarts(5 / 9f, 5),
                "Designed by the legendary wedge drawer <b>Giorgetto Giugiaro</b>, built by the body workshop <b>Bertone</b> and assembled by <b>Baur</b>, so many hands touched this promissing darling. But even in the hands of <b>Bernard Darniche</b> and <b>Bernard Béguin</b>, the <b>BMW M1</b> couldn't bring a decent win. Too heavy, too large and too unreliable, it still provided some shockingly good results. <b>Klaus Fritzinger</b> already had a taste for competition when he won football championships in his young years. He then hopped into a <b>Toyota</b> with his copilot <b>Henning Wünsch</b> and only briefly stepped into the <b>M1</b> to score a second place in the rally, showing off the car's power with a first stage win and a total of six stage wins."
            );

            RallyManager.AddCustomRally(
                group, 1983, Areas.KENYA, "Kenya", "Safari Rally", "Ari Vatanen",
                assembly, pilotPicturePath, 1983, 9, 0, 4, new int[] { 0, 8, 4, 6, 10 },
                new Weather[] { Weather.Sunset, Weather.Night, Weather.Morning, Weather.Afternoon, Weather.Sunset },
                ComputeRestarts(6 / 9f, 5),
                "Former <b>1981</b> <b>WRC</b> champion, <b>Ari Vatanen</b>, stepped out of his <b>Ford Escort MK2</b> and into the unbreakable <b>Opel Ascona 400</b> with his copilot <b>Terry Harryman</b> for his second ever <b>Rally Safari</b>. <b>Shekhar Mehta</b>, the previous year's winner broke his <b>Nissan 240RS</b> and left an open field for Ari while the previous year's <b>Africa</b> and <b>WRC</b> champion <b>Walter Röhrl</b> was busy with a <b>Group B</b> season for <b>Lancia</b>. Ari only drove the Ascona for that year before joining the insane <b>Group B</b> races aswell with the <b>Peugeot 205 Turbo 16</b>, but that was enough to place first and second in several rallies. Other very successfull pilots have stepped into this Ascona such as <b>Guy Fréquelin</b> in France, <b>Jimmy McRae</b> in Britain and <b>\"Miki\" Biasion</b> in Italy."
            );

            RallyManager.AddCustomRally(
                group, 1985, Areas.FINLAND, "Sweden", "Billingerundan", "Stig-Olov Walfridsson",
                assembly, pilotPicturePath, 0, 2, 1, 8, new int[] { 1, 2, 6, 4, 7, 3 },
                new Weather[] { Weather.Morning, Weather.Rain, Weather.Afternoon, Weather.Fog, Weather.Sunset, Weather.Night },
                ComputeRestarts(7 / 9f, 6),
                "A year after <b>Stig Blomqvist</b> won the <b>WRC</b> with an <b>Audi Quattro S1</b>, a young stunt driver named <b>Stig-Olov \"Stecka\" Walfridsson</b>, hopped into a <b>Volvo 240 Turbo</b> with his long time copilot <b>Gunner Barth</b> to win several small swedish rallies. His indestructible <b>Volvo</b> helped him show his skills before switching to an <b>Audi Quattro</b> for european junior rallies and later a <b>Mitsubishi Lancer</b> with which he won several <b>Group N</b> rally championships in the nineties before switching to rallycross after a high speed collision with a moose in <b>2006</b>. The reliable <b>\"Turbo Brick\"</b> was dirt cheap and fast on straights thanks to its turbo, making it a favourite among privateers."
            );

            RallyManager.AddCustomRally(
                group, 1988, Areas.SARDINIA, "France", "Tour de Corse", "Didier Auriol",
                assembly, pilotPicturePath, 1984, 4, 0, 2, new int[] { 7, 4, 2, 3, 9, 6 },
                new Weather[] {
                    Weather.Afternoon, Weather.Sunset, Weather.Morning, Weather.Afternoon, Weather.Fog, Weather.Sunset
                },
                ComputeRestarts(8 / 9f, 6),
                "You'd be worried if you discovered that the driver taking the wheel of your MG Metro 6R4 used to drive ambulances before he started racing in rally. Two years later in <b>1988</b> it was behind the wheel of a <b>Ford Sierra RS Cosworth</b> that <b>Didier Auriol</b> won his second <b>French Rally Championship</b> with <b>Bernard Occelli</b> as copilot, with whom he'd win a <b>WRC</b> title in <b>1994</b>. He went toe to toe with all wheel drive cars like the <b>Lancia Delta Integrale</b> and proved that rear wheel drive could still win on tarmac. You'd be surprised at how many accidents his reckless driving lead to when you hear his gentle tone and very strong french accent."
            );

            RallyManager.AddCustomRally(
                group, 1988, Areas.SARDINIA, "Italy", "Targa Florio", "Andrea Zanussi",
                assembly, pilotPicturePath, 1987, 1, 1, 10, new int[] { 3, 9, 1, 5, 7, 11 },
                new Weather[] {
                    Weather.Morning, Weather.Afternoon, Weather.Sunset, Weather.Morning, Weather.Afternoon, Weather.Sunset
                },
                ComputeRestarts(9 / 9f, 6),
                "The first man to go under the 11 minutes barrier at the <b>Pikes Peak hillclimb</b>, placing third at the overall in <b>1987</b>, <b>Andrea Zanussi</b> hopped into a <b>BMW M3</b> the next year with his copilot <b>Paolo Amati</b> for his last title in the <b>Italian Championship</b>. The <b>Grand Tourism</b> winning <b>BMW</b> proved to be a mighty steed, bringing him two first places and three second places, giving him the second place in the <b>Italian Championship</b> that year. The solid M3 platform has several championships under its belt in France and Belgium, its precise chassis gained popularity with privateers who still enjoyed it well after <b>2000</b>."
            );
        }

        public static void GenerateGroupBSeasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_B;

            // TODO : Finish designing rallies for group B
        }

        public static void GenerateGroupSSeasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_S;

            // TODO : Finish designing rallies for group S
        }

        public static void GenerateGroupASeasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_A;

            // TODO : Finish designing rallies for group A
        }
    }
}
