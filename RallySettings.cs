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
                stagesIndeces.Length,
                restarts,
                "UNLOCKABLE_" + year + "_BONUS",
                true,
                AIDriverSkillTables.AI_Skill.EASY
            );

            // setup rally
            season.Rallies.Add(new RallyData());
            season.Rallies[0].SetArea((int)area);
            season.Rallies[0].SetStageCount(stagesIndeces.Length);

            for (int i = 0; i < stagesIndeces.Length; i++)
            {
                int index = stagesIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, area);
                season.Rallies[0].SetStage(i, stage);
                season.Rallies[0].SetWeatherForStage(i, weathers[i]);
            }

            SaveManager.LoadSeasonData(this);

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

            RallyManager.AddCustomRally(
                group, 1981, Areas.SARDINIA, "Italy", "Rally Sanremo", "Michèle Mouton",
                assembly, pilotPicturePath, 1981, 2, 0, 0, new int[] { 2, 6, 10, 0 },
                new Weather[] { Weather.Sunset, Weather.Morning, Weather.Sunset, Weather.Afternoon }, ComputeRestarts(0 / 15f, 4),
                "It was during a snowy test drive that the <b>Audi</b> engineers saw a military AWD <b>Volkswagen Iltis</b> plow around and thought about grafting its transmission to a road car. And in <b>1981</b> they presented the <b>Audi Quatro</b> to the <b>WRC</b> season, piloted by a former french <b>Fiat 131</b> driver, <b>Michèle Mouton</b>, and her copilot <b>Fabrizia Pons</b>. They turned out to be the first in many fields : first woman pilot, all female crew and AWD car to win a <b>WRC</b> rally. Michèle finished second of the <b>WRC</b> in <b>1982</b> and even set a new record at the <b>Pikes Peak</b> with an <b>Audi Sport Quatro S1</b>, owning the nickname \"<b>Queen of the mountain</b>\"."
            );

            RallyManager.AddCustomRally(
                group, 1982, Areas.SARDINIA, "Portugal", "Somewhere in Portugal", "Ari Vatanen",
                assembly, pilotPicturePath, 1983, 6, 0, 8, new int[] { 0, 10, 4, 3, 2 },
                new Weather[] { Weather.Rain, Weather.Fog, Weather.Sunset, Weather.Afternoon, Weather.Night },
                ComputeRestarts(1 / 15f, 5),
                "With the upcomming <b>Group B</b> regulations, said to start in <b>1982</b>, <b>Ford</b> was looking to perpetuate their racing pedigree with a new model. Engineers asked for <b>all wheel drive</b> but Ford's direction said \"No. Ford race cars are <b>propulsion only</b>\" and so the project <b>Ford Escort RS1700T</b> was born in <b>1980</b>. Two years later, two prototypes were shipped to <b>Portugal</b> to be tested by <b>Ari Vatanen</b> and <b>Penti Airikkala</b>. Even if those cars were not very reliable, both pilots were unanimous on the performances of the turbo charged engines. The project was abandonned in <b>1983</b> but was split into two, the <b>RS200</b> for <b>Group B</b> and the <b>Sierra</b> for <b>Group 4</b>."
            );

            RallyManager.AddCustomRally(
                group, 1983, Areas.GERMANY, "West Germany", "Rallye Vorderpfalz", "Erwin Weber",
                assembly, pilotPicturePath, 1983, 16, 0, 10, new int[] { 11, 8, 3, 5, 9 },
                new Weather[] { Weather.Morning, Weather.Afternoon, Weather.Sunset, Weather.Morning, Weather.Afternoon },
                ComputeRestarts(2 / 15f, 5),
                "The new generation came swinging out the door. The young <b>Erwin Weber</b>, coached by the experienced <b>Gunter Wanger</b>, started an unstoppable ascension to the <b>German Championship</b> title. Out with the old <b>Opel Ascona 400</b>, in with the new <b>Opel Manta 400</b>, even if it retained a \"traditional\" layout. It held the top position of the <b>Rally-raid Paris-Dakar</b> for a whole week, dancing in front of the <b>AWD</b> cars even though it was a propulsion. <b>Erwin</b> went on to win the <b>German Championship</b> again in <b>1991</b> and even the <b>ERC</b> in <b>1992</b>, behind the wheel of a <b>Volkswagen</b> and of a <b>Mitsubishi</b>."
            );

            RallyManager.AddCustomRally(
                group, 1983, Areas.AUSTRALIA, "New Zealand", "Sanyo Rally of New Zealand", "Timo Salonen",
                assembly, pilotPicturePath, 1985, 15, 0, 10, new int[] { 5, 0, 11, 8, 6 },
                new Weather[] { Weather.Afternoon, Weather.Morning, Weather.Rain, Weather.Fog, Weather.Rain },
                ComputeRestarts(3 / 15f, 5),
                "The long rally specialist <b>Timo Salonen</b> with his copilot <b>Seppo Harjanne</b> hopped into the brand new <b>Nissan 240RS</b> in <b>1983</b>. He was used to <b>Nissans</b> and <b>Datsuns</b> but couldn't cope with the low reliability of this model. The <b>240RS</b> never won a single <b>WRC</b> or <b>ERC</b> rally but was seen winning local rallies in several countries. Timo finished second on this rally but his tenacity impressed <b>Peugeot</b> who signed him up for <b>1985</b>, year of his <b>WRC</b> title, after having spent five years hovering between the fifth and tenth position on the <b>WRC</b> leaderboard."
            );

            RallyManager.AddCustomRally(
                group, 1983, Areas.AUSTRALIA, "New Zealand", "Sanyo Rally of New Zealand", "Walter Röhrl",
                assembly, pilotPicturePath, 1983, 9, 0, 10, new int[] { 5, 0, 11, 8, 6 },
                new Weather[] { Weather.Afternoon, Weather.Morning, Weather.Rain, Weather.Fog, Weather.Rain },
                ComputeRestarts(4 / 15f, 5),
                "<b>Lancia</b>'s racing team director <b>Cesare Fiorio</b> saw the <b>Audi Quatro</b> dominate the beginning of <b>Group B</b> and started planning for a new rally car that could topple the giant. He went to <b>Walter Röhrl</b>, double <b>WRC</b> champion in <b>1980</b> with a <b>Fiat 131 Abarth</b> and in <b>1982</b> with an <b>Opel Ascona 400</b>. <b>Walter</b> agreed but only for a partial season with his copilot <b>Christian Geistdörfer</b>, picking the rallies where the new <b>Lancia 037 Rally</b> could perform decently given it was a propulsion. He opened the season with a <b>Monte Carlo</b> win and drove <b>Lancia</b> to victory with the help of <b>Markku Alén</b>. To this day <b>Walter</b> is the only driver to have won a <b>WRC</b>, <b>ERC</b> and <b>ARC</b> title."
            );

            RallyManager.AddCustomRally(
                group, 1984, Areas.GERMANY, "Czechoslovakia", "Rallye Škoda", "Ingvar Carlsson",
                assembly, pilotPicturePath, 1989, 7, 0, 4, new int[] { 5, 2, 3, 10, 6 },
                new Weather[] { Weather.Morning, Weather.Rain, Weather.Sunset, Weather.Rain, Weather.Afternoon },
                ComputeRestarts(5 / 15f, 5),
                "When <b>Achim Warmbold</b> created the <b>Mazda Rally Team Europe</b> he was looking for talented pilots to tame the very unstable chassis and torquey birotor engine of the <b>Mazda RX-7</b>. Given he had no support from <b>Mazda</b> headquarters, he had to find the pilots on his own and selected the swedish multi-champion <b>Ingvar Carlsson</b>. At the wheel of the <b>Group B</b> homologated <b>Mazda RX-7 Evo</b>, and with his copilot <b>Benny Melander</b>, Ingvar scored a win in <b>Poland</b> and several podiums in other rallies, including a second place in the <b>Rallye Škoda</b>. Overseas in the <b>SCCA</b>, <b>Rod Millen</b> won the <b>Pro Rally</b> category 3 times in the 80s with a <b>Mazda RX-7 4x4</b> that he modified himself. <b>MRT</b> went on to score other wins during the late 80s in <b>Sweden</b> and <b>New Zealand</b> with the <b>Mazda 323 4WD</b>."
            );

            RallyManager.AddCustomRally(
                group, 1984, Areas.FINLAND, "Finland", "1000 Lakes rally", "Ari Vatanen",
                assembly, pilotPicturePath, 1984, 1, 0, 2, new int[] { 0, 5, 6, 8, 2 },
                new Weather[] { Weather.Morning, Weather.Afternoon, Weather.Rain, Weather.Sunset, Weather.Morning },
                ComputeRestarts(6 / 15f, 5),
                "The head of <b>Peugeot Talbot Sport</b>, <b>Jean Todt</b>, was preparing a forey in the world of rallies. He hired <b>1981 WRC</b> champion <b>Ari Vatanen</b> and his copilot <b>Terry Harryman</b> to drive the brand new <b>Peugeot 205 Turbo 16</b> for the <b>1984</b> season. Ari had a habbit of celebrating his wins with a glass of milk and many were drank that year, as they won <b>The 1000 lakes rally</b>, the <b>Rally Sanremo</b> and the <b>RAC rally</b> in what <b>Jean Todt</b> called \"learning rallies\". The next year Ari pushed his car too far and had a horrible accident in <b>Argentina</b>, breaking many bones and pushing him out of the competition. In <b>1986</b> the <b>Group B</b> was banned and <b>Peugeot</b> brought Ari to the <b>Rallye Dakar</b> where he won the <b>1987</b> edition. The same year he finished second behind <b>Walter Röhrl</b> at the <b>Pikes Peak</b> challenge."
            );

            RallyManager.AddCustomRally(
                group, 1985, Areas.KENYA, "Kenya", "Safari Rally", "Juha Kankkunen",
                assembly, pilotPicturePath, 0, 13, 1, 4, new int[] { 0, 8, 4, 6, 10 },
                new Weather[] { Weather.Afternoon, Weather.Rain, Weather.Morning, Weather.Sunset, Weather.Afternoon },
                ComputeRestarts(7 / 15f, 5),
                "Driving straight out of his dad's barn, Juha Kankkunen hit the ground running. The 1985 Safari Rally was his first WRC win with his copilot Fred Gallagher in a Toyota Celica Twin-Cam Turbo nicknamed the \"King of Africa\". That year was a huge win for Toyota with a first and second place thanks to Björn Waldegård in Kenya and Ivory Coast. The advice Timo Makinen used to give him in his young days must have helped since he as the first consecutive WRC winner, winning in 1986 when replacing Ari Vatanen for Peugeot and in 1987 with Lancia. He came back in 2010 for Rally finland and placed an eighth position after eight years of interruption at the wheel of a Ford Focus WRC. He then built a piloting school in Finland where he teaches his wild techniques."
            );

            RallyManager.AddCustomRally(
                group, 1985, Areas.SARDINIA, "France", "Tour de Corse", "Jean Ragnotti",
                assembly, pilotPicturePath, 1985, 11, 0, 2, new int[] { 7, 4, 2, 3, 9 },
                new Weather[] { Weather.Afternoon, Weather.Morning, Weather.Sunset, Weather.Rain, Weather.Rain },
                ComputeRestarts(8 / 15f, 5),
                "For the <b>Renault 5 Turbo</b> to be homologated in <b>Group B</b> the manufacturer had to pull all the stops, reworking the engine with F1 turbo technology, new suspensions, a frame reworked to hit 900 Kg and more refined aerodynamics. Only 20 were made and who better to drive the new <b>Renault 5 Maxi Turbo</b> than <b>Jean Ragnotti</b> and his new copilot <b>Pierre Thimonier</b>, the man who made the glory of this car's ancestor. After the <b>1986 Group B</b> ban, the french version of the <b>FIA</b> started <b>Group F</b> which continued its spirit, enableing Jean to continue his career in the ninetees. He stopped his career in <b>1996</b> because he said that <b>\"modern cars aren't as fun\"</b>. In <b>2002</b> for the return of <b>Renault</b> in F1 he was invited to drive the <b>Renault Espace F1</b> prototype which only three people could ever get their hands on."
            );

            RallyManager.AddCustomRally(
                group, 1985, Areas.SARDINIA, "Italy", "Rally Sanremo", "Walter Röhrl",
                assembly, pilotPicturePath, 1984, 3, 0, 0, new int[] { 2, 6, 10, 0, 8, 5 },
                new Weather[] { Weather.Afternoon, Weather.Morning, Weather.Fog, Weather.Fog, Weather.Night, Weather.Afternoon },
                ComputeRestarts(9 / 15f, 6),
                "The latest evolution of the <b>Audi Quattro</b>, an extravagant aero package, an early anti-lag system, it's practically a supercar and with the ban on <b>Group B</b> in <b>1986</b> it's the most powerfull rally car ever made, the last of the <b>Group B</b> monsters, and it fit <b>Walter Röhr</b>l and his copilot <b>Christian Geistdörfer</b> like a glove. He was a perfectionist pilot who didn't like to be at a disadvantage and begrudged participating in complex rallies like the <b>RAC</b> where he rolled over his <b>Audi Sport Quattro S1</b> that year, or the finish rallies that he deemed \"too dangerous\". He won every rally where he drove the <b>S1</b> that year and beat <b>Andrea Zanussi</b> at the <b>Pikes Peak</b> with a modified version in <b>1987</b>. After an impressive <b>WRC</b> career, having won <b>WRC</b> rallies with cars from four different constructors, he was hired by <b>Porsche</b> as a test pilot in <b>1992</b>. He's still considered as one of the best rally pilots in the world."
            );

            RallyManager.AddCustomRally(
                group, 1986, Areas.SARDINIA, "Italy", "Somewhere around Fiorano", "Dario Benuzzi",
                assembly, pilotPicturePath, 0, 10, 5, 4, new int[] { 5, 8, 0, 11, 1, 4 },
                new Weather[] { Weather.Sunset, Weather.Rain, Weather.Morning, Weather.Rain, Weather.Night, Weather.Afternoon },
                ComputeRestarts(10 / 15f, 6),
                "The mechanic, turned test driver, Dario Benuzzi was shocked when the Ferrari 288 GTO Evoluzione came on the Fiorano test track, and that was something since Dario tested all cars since the Ferrari Dino, including F1 cars. The 288 was a civilized car on the low and felt like a turbo jet on the high, but with a bi-turbo V8, which was a first for Ferrari, an injection system pulled straight from F1 cars and a spectacular engine cartography, the 288 Evo's high turbo lag would \"throw you in the stratosphere\" and felt like \"hell started at 5001 RPM\". The aerodynamics were horrible but the car still pulled a 0-100 km/h in 2,8 seconds. The late development was stopped abruptly when the Group B ban came in and only 5 of those were ever made, but the efforts weren't lost as the car was used as a base for the development of the F40. Dario helped tame the wild Ferrari horses into civilized cars for 40 years."
            );

            RallyManager.AddCustomRally(
                group, 1986, Areas.NORWAY, "Sweden", "South Swedish Rally", "Stig Blomqvist",
                assembly, pilotPicturePath, 1984, 5, 0, 0, new int[] { 11, 8, 10, 0, 5, 9 },
                new Weather[] { Weather.Fog, Weather.Snow, Weather.Afternoon, Weather.Sunset, Weather.Morning, Weather.Afternoon },
                ComputeRestarts(11 / 15f, 6),
                "Ford wanted to reignite its glory in rally and brought the Ford RS200 to life from a brand new chassis, new internals, an underpowered engine and the lessons they learned developing the Ford Escort RS1700T. But a tragedy soon clouded the entirety of Group B, during the Rallye de Portugal, Joaquim Santos piloting an RS200 missed a turn and hit the crowd, killing three people and injuring around thirty. This prompted the WRC to cancel Group B, so the RS200 was now competing in local rallies and moved to rally cross with an evolved version having some success. This is where 1984 WRC champion Stig Blomqvist comes in with his copilot Bruno Berglund to win the South Swedish Rally. His shy nature didn't stop him from holding many records : most WRC seasons (32), most wins on snow (equal to Marcus Grönholm), two Race of Champions titles and a Pikes Peak win in 2004 with an evolved version of the RS200."
            );

            RallyManager.AddCustomRally(
                group, 1986, Areas.KENYA, "North Africa", "Rallye Paris-Dakar", "René Metge",
                assembly, pilotPicturePath, 1986, 14, 0, 6, new int[] { 4, 0, 9, 1, 3, 7 },
                new Weather[] { Weather.Afternoon, Weather.Fog, Weather.Night, Weather.Sunset, Weather.Fog, Weather.Morning },
                ComputeRestarts(12 / 15f, 6),
                "After their win in 1984 with a 911 SC 4x4, Porsche decided to make a super car and thew it at the biggest rally raid they could think of : the Paris-Dakar. In 1985 all three of the cars broke down but in 1986 the two teams lead by the long rally specialist René Metge, nicknamed the \"cowboy from Malakoff\", and Jacky Ickx finished first and second. That edition of the Paris-Dakar was the hardest and deadliest of its story. On the second day Yazuko Keneko was ran over during a liaison, Jean-Michel Baron broke his spine on a liaison and ended up paralized and Gianpaolo Marianoni ruptured his spleen, got back on his bike and finished the rally but died two days later. Half of the teams with cars were out of the race but the desert stages had barely started when a tragic event happened. The hellicopter transporting the organizer of the event, Thierry Sabine, crashed in a dune and killed the 5 people on board."
            );

            RallyManager.AddCustomRally(
                group, 1986, Areas.FINLAND, "Finland", "Mänttä 200-ajo", "Malcolm Wilson",
                assembly, pilotPicturePath, 1986, 0, 0, 0, new int[] { 8, 11, 5, 0, 10, 9 },
                new Weather[] { Weather.Fog, Weather.Morning, Weather.Afternoon, Weather.Rain, Weather.Sunset, Weather.Night },
                ComputeRestarts(13 / 15f, 6),
                "<b>MG</b> wanted to enter the big leagues with a <b>Group B</b> homologated car, they studied the rally leaders and settled on a base similar to the <b>MG Metro</b>, a small chassis akin to the <b>Renault 5</b>, an AWD system inspired by the <b>Audi Quattro</b>, a naturally aspirated V6 giving a stable torque from low RPM and max power at the redline making this a zippy screamer that sticks to the road like glue. Unfortunately the <b>MG Metro 6R4</b> came too late and could not compete against the likes of the <b>205</b>, its best <b>WRC</b> result being a third place at the <b>RAC rally</b>. But the car won on local championships with the <b>1978-1979</b> double british rally champion <b>Malcolm Wilson</b>, who had a knack for prototypes after having tested the <b>Ford Escort RS1700T</b>, and his copilot <b>Nigel Harris</b>. He went on to found the legendary <b>M-Sport</b> rally team after retiring from the competition. Another notable win is in the french rally championship with pilot <b>Didier Auriol</b>."
            );

            RallyManager.AddCustomRally(
                group, 1986, Areas.SARDINIA, "Italy", "Rally Sanremo", "Markku Alén",
                assembly, pilotPicturePath, 1984, 4, 0, 0, new int[] { 2, 6, 10, 0, 8, 5 },
                new Weather[] { Weather.Afternoon, Weather.Night, Weather.Morning, Weather.Sunset, Weather.Fog, Weather.Fog },
                ComputeRestarts(14 / 15f, 6),
                "Tensions were rising in <b>Group B</b>, <b>Markku Alén</b> and <b>Henri Toivonen</b> were leading the charge against <b>Peugeot</b> with the new <b>Lancia Delta S4</b>, based on the <b>205</b>, loaded with a turbo charger and a compressor, competitive on dirt, gravel and asphalt. This race for power combined with very poor security, as seen with the <b>RS200</b> accident in <b>Portugal</b>, were scaring the pilots who decided to come together for the <b>Tour de Corse</b> and tell the <b>FIA</b> to tone it down. At the end of the <b>Safari rally</b>, Markku was already pointing it out : <b>\"I come in sideways at 200km/h, it isn't funny anymore\"</b>. Tragedy struck when the young <b>Henri Toivonen</b> crashed into a tree setting his car on fire, only the chassis was recovered. Markku had been around in the top ten of the <b>WRC</b> for years and finished second in <b>1986</b> with his copilot <b>Ilkka Kivimäki</b>."
            );

            RallyManager.AddCustomRally(
                group, 1987, Areas.GERMANY, "Germany", "Somewhere in Bavaria", "Walter Röhrl",
                assembly, pilotPicturePath, 1984, 8, 2, 6, new int[] { 8, 1, 10, 6, 8, 2 },
                new Weather[] { Weather.Rain, Weather.Fog, Weather.Afternoon, Weather.Fog, Weather.Sunset, Weather.Rain },
                ComputeRestarts(15 / 15f, 6),
                "The head of <b>Audi Ferdinand Piëch</b> was pushing the development teams to keep tinkering with the <b>S1</b> after the <b>Group B</b> ban by the <b>FIA</b>, but a group of engineers saw the writing on the wall, their front-engined car with a heavy five cylinders engine would never hold against the mid-engined <b>Peugeot 205</b> and <b>Lancia S4</b>. <b>Group S</b> needed a new car and so the engineering team under <b>Roland Gumpert</b> decided to phone <b>Porsche</b> for help and developped the <b>Audi Sport Quattro RS 001</b>, a short mid-engined prototype. <b>Walter Röhrl</b> tested it somewhere in bavaria around <b>1987</b> and said that this was now a true weapon againts its rivals. Journalists managed to take a few pictures and broke the story to the news in <b>Austria</b>. <b>Piëch</b> was so furious that he ordered the prototypes to be destroyed under his eyes, but he didn't know that a prototype lay dormant at <b>Neckarsul</b>."
                );

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
