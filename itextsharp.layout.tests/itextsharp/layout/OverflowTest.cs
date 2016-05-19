using System;
using System.Text;
using Java.IO;
using NUnit.Framework;
using iTextSharp.IO.Font;
using iTextSharp.Kernel.Font;
using iTextSharp.Kernel.Pdf;
using iTextSharp.Kernel.Utils;
using iTextSharp.Layout.Element;
using iTextSharp.Test;

namespace iTextSharp.Layout
{
	public class OverflowTest : ExtendedITextTest
	{
		public const String sourceFolder = "../../resources/itextsharp/layout/OverflowTest/";

		public const String destinationFolder = "test/itextsharp/layout/OverflowTest/";

		[TestFixtureSetUp]
		public static void BeforeClass()
		{
			CreateDestinationFolder(destinationFolder);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		public virtual void TextOverflowTest01()
		{
			String outFileName = destinationFolder + "textOverflowTest01.pdf";
			String cmpFileName = sourceFolder + "cmp_textOverflowTest01.pdf";
			PdfDocument pdfDocument = new PdfDocument(new PdfWriter(new FileOutputStream(outFileName
				, FileMode.Create)));
			Document document = new Document(pdfDocument);
			StringBuilder text = new StringBuilder();
			for (int i = 0; i < 1000; i++)
			{
				text.Append("This is a waaaaay tooo long text...");
			}
			Paragraph p = new Paragraph(text.ToString()).SetFont(PdfFontFactory.CreateFont(FontConstants
				.HELVETICA));
			document.Add(p);
			document.Close();
			NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(outFileName, cmpFileName
				, destinationFolder, "diff"));
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		public virtual void TextOverflowTest02()
		{
			String outFileName = destinationFolder + "textOverflowTest02.pdf";
			String cmpFileName = sourceFolder + "cmp_textOverflowTest02.pdf";
			PdfDocument pdfDocument = new PdfDocument(new PdfWriter(new FileOutputStream(outFileName
				, FileMode.Create)));
			Document document = new Document(pdfDocument);
			iTextSharp.Layout.Element.Text overflowText = new iTextSharp.Layout.Element.Text(
				"This is a long-long and large text which will not overflow").SetFontSize(19).SetFontColor
				(iTextSharp.Kernel.Color.Color.RED);
			iTextSharp.Layout.Element.Text followText = new iTextSharp.Layout.Element.Text("This is a text which follows overflowed text and will be wrapped"
				);
			document.Add(new Paragraph().Add(overflowText).Add(followText));
			document.Close();
			NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(outFileName, cmpFileName
				, destinationFolder, "diff"));
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		public virtual void TextOverflowTest03()
		{
			String outFileName = destinationFolder + "textOverflowTest03.pdf";
			String cmpFileName = sourceFolder + "cmp_textOverflowTest03.pdf";
			PdfDocument pdfDocument = new PdfDocument(new PdfWriter(new FileOutputStream(outFileName
				, FileMode.Create)));
			Document document = new Document(pdfDocument);
			iTextSharp.Layout.Element.Text overflowText = new iTextSharp.Layout.Element.Text(
				"This is a long-long and large text which will overflow").SetFontSize(25).SetFontColor
				(iTextSharp.Kernel.Color.Color.RED);
			iTextSharp.Layout.Element.Text followText = new iTextSharp.Layout.Element.Text("This is a text which follows overflowed text and will not be wrapped"
				);
			document.Add(new Paragraph().Add(overflowText).Add(followText));
			document.Close();
			NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(outFileName, cmpFileName
				, destinationFolder, "diff"));
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		public virtual void TextOverflowTest04()
		{
			String outFileName = destinationFolder + "textOverflowTest04.pdf";
			String cmpFileName = sourceFolder + "cmp_textOverflowTest04.pdf";
			PdfDocument pdfDocument = new PdfDocument(new PdfWriter(new FileOutputStream(outFileName
				, FileMode.Create)));
			Document document = new Document(pdfDocument);
			document.Add(new Paragraph("ThisIsALongTextWithNoSpacesSoSplittingShouldBeForcedInThisCase"
				).SetFontSize(20));
			document.Close();
			NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(outFileName, cmpFileName
				, destinationFolder, "diff"));
		}
	}
}
