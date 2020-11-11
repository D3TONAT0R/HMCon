using ASCReader;
using ASCReader.Export;
using ASCReader.Import;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.IO;
using System.Net;

namespace HMConTests {
	public class Tests {

		string sampleASCFile = "sample-maps.zh.asc";
		string sampleCroppedASCFile = "sample-cropped-maps.zh.asc";
		string sampleHeightmapFile = "sample-hm.png";
		string sampleMCAFile = "sample-mca.16.26.mca";

		string inputPath;
		string outputPath;

		[SetUp]
		public void Setup() {
			string filePath = AppDomain.CurrentDomain.BaseDirectory;
			for(int i = 0; i < 4; i++) filePath = Directory.GetParent(filePath).FullName;
			inputPath = Path.Combine(filePath, "TestFiles", "in");
			outputPath = Path.Combine(filePath, "TestFiles", "out");
			Program.Initialize(AppContext.BaseDirectory + "..\\..\\..\\..\\HMConApp\\bin\\Debug\\netcoreapp3.1\\");
		}

		[Test]
		public void ExportDefaultHillshadeTest() {
			Assert.Pass();
		}

		[Test]
		public void TestASCExport() {
			ASCData data = new ASCData(Path.Combine(inputPath, sampleASCFile));
			var sampleLocations = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			ExportUtility.ExportFile(inputPath, ExportUtility.GetFormatFromIdenfifier("ASC"), data, Path.Combine(outputPath, sampleASCFile), "", new ExportOptions(), new ASCReader.Util.Bounds(0,0,data.ncols-1,data.nrows-1));
			data = new ASCData(Path.Combine(outputPath, sampleASCFile));
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			Assert.AreEqual(sourceSamples, exportedSamples);
		}

		[Test]
		public void TestCroppedASCExport() {
			ASCData data = new ASCData(Path.Combine(inputPath, sampleASCFile));
			int x1 = 250;
			int y1 = 1180;
			int x2 = 850;
			int y2 = 1920;
			var sampleLocations = GetSampleLocations(x1, y1, x2, y2);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			ExportUtility.ExportFile(inputPath, ExportUtility.GetFormatFromIdenfifier("ASC"), data, Path.Combine(outputPath, sampleCroppedASCFile), "", new ExportOptions(), new ASCReader.Util.Bounds(x1, y1, x2, y2));
			data = new ASCData(Path.Combine(outputPath, sampleCroppedASCFile));
			for(int i = 0; i < sampleLocations.Length; i++) {
				sampleLocations[i].x -= x1;
				sampleLocations[i].y -= y1;
			}
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			Assert.AreEqual(sourceSamples, exportedSamples);
		}

		[Test]
		public void TestHeightmapHandling() {
			ASCData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleHeightmapFile), "png");
			var sampleLocations = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			ExportUtility.ExportFile(outputPath, ExportUtility.GetFormatFromIdenfifier("IMG_PNG-HM"), data, Path.Combine(outputPath, sampleHeightmapFile), "", new ExportOptions(), new ASCReader.Util.Bounds(0, 0, data.ncols - 1, data.nrows - 1));
			data = ImportManager.ImportFile(Path.Combine(outputPath, sampleHeightmapFile), "png");
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			for(int i = 0; i < sourceSamples.Length; i++) {
				Assert.AreEqual(sourceSamples[i], exportedSamples[i], 0.0001d, $"Index {i}, location [{sampleLocations[i].x},{sampleLocations[i].y}]");
			}
		}

		[Test]
		public void TestMCAFileHandling() {
			ASCData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleMCAFile), "mca");
			data.lowPoint = 0;
			data.highPoint = 255;
			var sampleLocations = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			ExportUtility.ExportFile(inputPath, ExportUtility.GetFormatFromIdenfifier("MCA"), data, Path.Combine(outputPath, sampleMCAFile), "", new ExportOptions(), new ASCReader.Util.Bounds(0, 0, data.ncols - 1, data.nrows - 1));
			ExportUtility.ExportFile(inputPath, ExportUtility.GetFormatFromIdenfifier("IMG_PNG-HS"), data, Path.Combine(outputPath, sampleMCAFile), "_hs", new ExportOptions(), new ASCReader.Util.Bounds(0, 0, data.ncols - 1, data.nrows - 1));
			ExportUtility.ExportFile(inputPath, ExportUtility.GetFormatFromIdenfifier("IMG_PNG-HM"), data, Path.Combine(outputPath, sampleMCAFile), "_hm", new ExportOptions(), new ASCReader.Util.Bounds(0, 0, data.ncols - 1, data.nrows - 1));
		}

		(int x, int y)[] GetSampleLocations(int maxX, int maxY) {
			return GetSampleLocations(0, 0, maxX-1, maxY-1);
		}

		(int x, int y)[] GetSampleLocations(int x1, int y1, int x2, int y2) {
			var sampleLocations = new (int x, int y)[65];
			for(int x = 0; x < 8; x++) {
				for(int y = 0; y < 8; y++) {
					int i = y * 8 + x;
					sampleLocations[i].x = x1 + (int)((x2-x1) * (x / 8f));
					sampleLocations[i].y = y1 + (int)((y2-y1) * (y / 8f));
				}
			}
			sampleLocations[64].x = x2;
			sampleLocations[64].y = y2;
			return sampleLocations;
		}

		float[] GetHeightSamples(ASCData data, (int x, int y)[] locations) {
			float[] samples = new float[locations.Length];
			for(int i = 0; i < locations.Length; i++) {
				var l = locations[i];
				samples[i] = data.GetData(l.x, l.y);
			}
			return samples;
		}
	}
}