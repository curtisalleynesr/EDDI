﻿using EddiSpeechService;
using EddiVoiceAttackResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class SpeechUnitTests : TestBase
    {
        PrivateObject speechService = new PrivateObject(SpeechService.Instance);

        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestSpeechPriorityIfInOrder()
        {
            speechService.SetFieldOrProperty("speechQueue", new SpeechQueue());
            SpeechQueue speechQueue = (SpeechQueue)speechService.GetFieldOrProperty("speechQueue");

            EddiSpeech speech1 = new EddiSpeech("Priority 1", null, 1);
            EddiSpeech speech2 = new EddiSpeech("Priority 3", null, 3);

            speechQueue.Enqueue(speech1);
            speechQueue.Enqueue(speech2);

            PrivateObject speechQueueObject = new PrivateObject(speechQueue);
            speechQueue.TryDequeue(out EddiSpeech result1);
            speechQueue.TryDequeue(out EddiSpeech result2);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            // Speech should be output in order of priority
            Assert.AreEqual("Priority 1", result1.message);
            Assert.AreEqual("Priority 3", result2.message);
        }

        [TestMethod]
        public void TestSpeechPriorityIfOutOfOrder()
        {
            speechService.SetFieldOrProperty("speechQueue", new SpeechQueue());
            SpeechQueue speechQueue = (SpeechQueue)speechService.GetFieldOrProperty("speechQueue");

            EddiSpeech speech1 = new EddiSpeech("Priority 3", null, 3);
            EddiSpeech speech2 = new EddiSpeech("Priority 1", null, 1);

            speechQueue.Enqueue(speech1);
            speechQueue.Enqueue(speech2);

            PrivateObject speechQueueObject = new PrivateObject(speechQueue);
            speechQueue.TryDequeue(out EddiSpeech result1);
            speechQueue.TryDequeue(out EddiSpeech result2);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            // Speech should be output in order of priority
            Assert.AreEqual("Priority 1", result1.message);
            Assert.AreEqual("Priority 3", result2.message);
        }

        [TestMethod]
        public void TestActiveSpeechPriority()
        {
            EddiSpeech priority5speech = new EddiSpeech("Priority 5", null, 5);
            EddiSpeech priority4speech = new EddiSpeech("Priority 2", null, 4);
            EddiSpeech priority2speech = new EddiSpeech("Priority 4", null, 2);
            EddiSpeech priority1speech = new EddiSpeech("Priority 1", null, 1);

            // Set up priority 5 speech
            speechService.SetFieldOrProperty("activeSpeechPriority", priority5speech.priority);
            Assert.AreEqual(5, (int)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 5 speech IS interrupted by priority 4 speech.
            Assert.IsTrue((bool)speechService.Invoke("checkSpeechInterrupt", new object[] { priority4speech.priority }));

            // Set up priority 4 speech
            speechService.SetFieldOrProperty("activeSpeechPriority", priority4speech.priority);
            Assert.AreEqual(4, (int)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 4 speech IS NOT interrupted by priority 2 speech.
            Assert.IsFalse((bool)speechService.Invoke("checkSpeechInterrupt", new object[] { priority2speech.priority }));

            // Check that priority 4 speech IS interrupted by priority 1 speech.
            Assert.IsTrue((bool)speechService.Invoke("checkSpeechInterrupt", new object[] { priority1speech.priority }));
        }

        [TestMethod]
        public void TestClearSpeechQueue()
        {
            EddiSpeech speech = new EddiSpeech("Priority 3", null, 3);
            SpeechQueue speechQueue = new SpeechQueue();
            Assert.IsTrue(speechQueue.priorityQueues.ElementAtOrDefault(speech.priority) != null);

            speechQueue.priorityQueues[speech.priority].Enqueue(speech);
            Assert.AreEqual(1, speechQueue.priorityQueues[speech.priority].Count);

            speechQueue.DequeueAllSpeech();

            Assert.AreEqual(0, speechQueue.priorityQueues[speech.priority].Count);
        }

        [TestMethod]
        public void TestFilterSpeechQueue()
        {
            EddiSpeech speech1 = new EddiSpeech("Jumped", null, 3, null, false, "FSD engaged");
            EddiSpeech speech2 = new EddiSpeech("Refueled", null, 3, null, false, "Ship refueled");
            EddiSpeech speech3 = new EddiSpeech("Scanned", null, 3, null, false, "Body scan");

            SpeechQueue speechQueue = new SpeechQueue();
            Assert.IsTrue(speechQueue.priorityQueues.ElementAtOrDefault(3) != null);
            speechQueue.priorityQueues[speech1.priority].Enqueue(speech1);
            speechQueue.priorityQueues[speech2.priority].Enqueue(speech2);
            speechQueue.priorityQueues[speech3.priority].Enqueue(speech3);

            Assert.AreEqual(3, speechQueue.priorityQueues[3].Count);

            speechQueue.DequeueSpeechOfType("Body scan");

            Assert.AreEqual(2, speechQueue.priorityQueues[3].Count);
            if (speechQueue.priorityQueues[3].TryDequeue(out EddiSpeech result1))
            {
                Assert.AreEqual("FSD engaged", result1.eventType);
            }
            if (speechQueue.priorityQueues[3].TryDequeue(out EddiSpeech result2))
            {
                Assert.AreEqual("Ship refueled", result2.eventType);
            }
        }

        [TestMethod]
        public void TestPathingString1()
        {
            string pathingString = @"There are [4;5] lights";
            List<string> pathingOptions = new List<string>() {
                "There are 4 lights"
                , "There are 5 lights"
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString2()
        {
            string pathingString = @"There are [4;5;] lights";
            List<string> pathingOptions = new List<string>() {
                "There are 4 lights"
                , "There are 5 lights"
                , "There are  lights"
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString3()
        {
            string pathingString = @"There [are;might be;could be] [4;5;] lights;It's dark in here;";
            List<string> pathingOptions = new List<string>() {
                "There are 4 lights"
                , "There are 5 lights"
                , "There are  lights"
                ,"There might be 4 lights"
                , "There might be 5 lights"
                , "There might be  lights"
                ,"There could be 4 lights"
                , "There could be 5 lights"
                , "There could be  lights"
                , "It's dark in here"
                , ""
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString4()
        {
            string pathingString = @";;;;;;Seven;;;";
            List<string> pathingOptions = new List<string>() {
                ""
                , "Seven"
            };

            int sevenCount = 0;
            for (int i = 0; i < 10000; i++)
            {
                string pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                if (pathedString == "Seven")
                {
                    sevenCount++;
                }
            }

            Assert.IsTrue(sevenCount > 750);
            Assert.IsTrue(sevenCount < 1500);
        }

        [TestMethod]
        public void TestPathingString5()
        {
            string pathingString = @"You leave me [no choice].";
            List<string> pathingOptions = new List<string>() {
                "You leave me no choice."
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString6()
        {
            string pathingString = @"[There can be only one.]";
            List<string> pathingOptions = new List<string>() {
                "There can be only one."
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString7()
        {
            string pathingString = @"[{TXT:Ship model} {TXT:Ship callsign (spoken)};This is {TXT:Ship model} {TXT:Ship callsign (spoken)}] [requesting docking permission;requesting docking clearance;requesting permission to dock;requesting clearance to dock].";
            List<string> pathingOptions = new List<string>() {
                "{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking permission."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking permission."
                ,"{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking clearance."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking clearance."
                ,"{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting clearance to dock."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting clearance to dock."
                ,"{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting permission to dock."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting permission to dock."
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestSectorTranslations()
        {
            Assert.AreEqual("Swoiwns N Y dash B a 95 dash 0", Translations.StarSystem("Swoiwns NY-B a95-0"));
            Assert.AreEqual("P P M 5 2 8 7", Translations.StarSystem("PPM 5287"));
        }

        [TestMethod]
        public void TestSpeechHumanize1()
        {
            Assert.AreEqual("well over minus 12 thousand", Translations.Humanize(-12345));
        }

        [TestMethod]
        public void TestSpeechHumanize2()
        {
            Assert.AreEqual(null, Translations.Humanize(null));
        }

        [TestMethod]
        public void TestSpeechHumanize3()
        {
            Assert.AreEqual("zero", Translations.Humanize(0));
        }

        [TestMethod]
        public void TestSpeechHumanize4()
        {
            Assert.AreEqual("0.16", Translations.Humanize(0.15555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize5()
        {
            Assert.AreEqual("0.016", Translations.Humanize(0.015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize6()
        {
            Assert.AreEqual("0.0016", Translations.Humanize(0.0015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize7()
        {
            Assert.AreEqual("minus 51 million", Translations.Humanize(-51000000));
        }

        [TestMethod]
        public void TestSpeechHumanize8()
        {
            Assert.AreEqual("just over 51 million", Translations.Humanize(51000001));
        }

        [TestMethod]
        public void TestSpeechHumanize9()
        {
            Assert.AreEqual("10 thousand", Translations.Humanize(10000));
        }

        [TestMethod]
        public void TestSpeechHumanize10()
        {
            Assert.AreEqual("100 thousand", Translations.Humanize(100000));
        }

        [TestMethod]
        public void TestSpeechHumanize11()
        {
            Assert.AreEqual("minus 0.16", Translations.Humanize(-0.15555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize12()
        {
            Assert.AreEqual("minus 0.016", Translations.Humanize(-0.015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize13()
        {
            Assert.AreEqual("minus 0.0016", Translations.Humanize(-0.0015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize14()
        {
            Assert.AreEqual("over minus 12", Translations.Humanize(-12.1M));
        }

        [TestMethod]
        public void TestSpeechHumanize15()
        {
            Assert.AreEqual("just over minus 12", Translations.Humanize(-12.01M));
        }

        [TestMethod]
        public void TestSpeechHumanize16()
        {
            Assert.AreEqual("just over 436 trillion", Translations.Humanize(4.36156E14M));
        }

        [TestMethod]
        public void TestSpeechHumanize17()
        {
            Assert.AreEqual("well over 945 billion", Translations.Humanize(9.4571E11M));
        }

        [TestMethod]
        public void TestSpeechHumanize18()
        {
            Assert.AreEqual("over 912 quadrillion", Translations.Humanize(9.1235E17M));
        }

        public void TestSpeechHumanize19()
        {
            Assert.AreEqual("nearly 646 thousand", Translations.Humanize(6.459E5M));
        }

        [TestMethod]
        public void TestTranslationVesper()
        {
            Assert.AreEqual(Translations.StarSystem("VESPER-M4"), "Vesper M 4");
        }

        [TestMethod]
        public void TestSpeechQueue_DequeueSpeechOfType()
        {
            PrivateObject privateObject = new PrivateObject(SpeechQueue.Instance);
            privateObject.Invoke("DequeueAllSpeech", System.Array.Empty<object>());
            privateObject.Invoke("Enqueue", new object[] { new EddiSpeech("Test speech 1", null, 3, null, false, null) });
            privateObject.Invoke("Enqueue", new object[] { new EddiSpeech("Test speech 2", null, 4, null, false, "Hull damaged") });
            privateObject.Invoke("Enqueue", new object[] { new EddiSpeech("Test speech 3", null, 3, null, false, "Body scanned") });

            List<ConcurrentQueue<EddiSpeech>> priorityQueues = (List<ConcurrentQueue<EddiSpeech>>)privateObject.GetFieldOrProperty("priorityQueues");
            Assert.AreEqual(3, priorityQueues.SelectMany(q => q).Count());
            try
            {
                // Only the speech of type "Hull damaged" should be removed, null types and other types should remain in place.
                privateObject.Invoke("DequeueSpeechOfType", new object[] { "Hull damaged" });
                Assert.AreEqual(2, priorityQueues.SelectMany(q => q).Count());
                // Verify that the order of remaining speech of the same priority is unchanged.
                Assert.AreEqual("Test speech 1", priorityQueues[3].First().message);
                Assert.AreEqual("Test speech 3", priorityQueues[3].Last().message);
            }
            catch (System.Exception)
            {
                Assert.Fail();
            }
            privateObject.Invoke("DequeueAllSpeech", System.Array.Empty<object>());
        }

        [TestMethod]
        public void TestSpeechServiceEscaping1()
        {
            // Test escaping for invalid ssml.
            var line = @"<invalid>test</invalid> <invalid withattribute='attribute'>test2</invalid>";
            var result = SpeechService.escapeSsml(line);
            Assert.AreEqual("&lt;invalid&gt;test&lt;/invalid&gt; &lt;invalid withattribute='attribute'&gt;test2&lt;/invalid&gt;", result);
        }

        [TestMethod]
        public void TestSpeechServiceEscaping2()
        {
            // Test escaping for double quotes, single quotes, and <phoneme> ssml commands. XML characters outside of ssml elements are escaped.
            var line = @"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet='ipa' ph='ˈdezɦrə'>Dezhra</phoneme> & Co's shop";
            var result = SpeechService.escapeSsml(line);
            Assert.AreEqual("<phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet='ipa' ph='ˈdezɦrə'>Dezhra</phoneme> &amp; Co&apos;s shop", result);
        }

        [TestMethod]
        public void TestSpeechServiceEscaping3()
        {
            // Test escaping for <break> elements. XML characters outside of ssml elements are escaped.
            var line = @"<break time=""100ms""/>He said ""Foo"".";
            var result = SpeechService.escapeSsml(line);
            Assert.AreEqual("<break time=\"100ms\"/>He said &quot;Foo&quot;.", result);
        }

        [TestMethod]
        public void TestSpeechServiceEscaping4()
        {
            // Test escaping for Cereproc unique <usel> and <spurt> elements
            var line = @"<spurt audio='g0001_004'>cough</spurt> This is a <usel variant=""1"">test</usel> sentence.";
            var result = SpeechService.escapeSsml(line);
            Assert.AreEqual(line, result);
        }
    }
}
