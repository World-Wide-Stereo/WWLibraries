using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PdfiumViewer;
using ww.Utilities;
using ww.Utilities.Extensions;
using Word = Microsoft.Office.Interop.Word;

public class libword
{
    private Word.Application _wrdApp;
    public Word.Application wrdApp
    {
        get { return _wrdApp ?? (_wrdApp = new Word.Application()); }
    }

    private object oMissing = System.Reflection.Missing.Value;
    private object oFalse;
    private object oTrue;
    private object oEnd = "\\endofdoc";

    #region public string rtf2word(object rtffile, object wordfile, object template)
    public string rtf2word(object rtffile, object wordfile, object template)
    {
        Object oMissing = new object();
        oMissing = System.Reflection.Missing.Value;
        Object oFalse = new object();
        oFalse = false;
        Object wdFormatDocument = new object();
        wdFormatDocument = 0;

        Word.Application wrdApp;
        Word._Document wrdDoc;
        wrdApp = new Word.Application();
        wrdDoc = wrdApp.Documents.Open(ref rtffile, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        wrdDoc.set_AttachedTemplate(ref template);
        wrdDoc.SaveAs(ref wordfile, ref wdFormatDocument, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        string plainText = wrdDoc.Content.Text;
        wrdDoc.Close(ref oFalse, ref oMissing, ref oMissing);
        wrdApp.Quit(ref oFalse, ref oMissing, ref oMissing);
        return plainText;
    }
    #endregion

    #region public string doc2word(object rtffile, object wordfile, object template)
    public string doc2word(object rtffile, object wordfile, object template)
    {
        Word.Application wrdApp = new Word.Application();
        Word._Document wrdDoc = wrdApp.Documents.Open(ref rtffile, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        wrdDoc.set_AttachedTemplate(ref template);
        wrdDoc.Save();
        string plainText = wrdDoc.Content.Text;
        wrdDoc.Close(ref oFalse, ref oMissing, ref oMissing);
        wrdApp.Quit(ref oFalse, ref oMissing, ref oMissing);
        return plainText;
    }
    #endregion

    public void createworddoc(object contents, bool visible = true)
    {
        wrdApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        wrdApp.ActiveDocument.Range(ref oMissing, ref oMissing).Text = contents.ToString();
        wrdApp.Visible = visible;
    }

    public void openworddoc(object wordfile, object read_only, bool visible, bool paste, float? fontSize = null, bool insertPageBreakBeforePaste = false)
    {
        openworddoc(wordfile, read_only, visible);
        if (fontSize != null)
            wrdApp.ActiveDocument.Range(wrdApp.ActiveDocument.Content.Start, wrdApp.ActiveDocument.Content.End).Font.Size = (float)fontSize;
        if (insertPageBreakBeforePaste)
            insertNextPageBreak();
        if (paste)
        {
            object obj = wrdApp.ActiveDocument.Bookmarks[oEnd].Range;
            Word.Range r = (Word.Range)obj;
            r.Select();
            wrdApp.ActiveWindow.Selection.Paste();
        }
    }

    public void openworddoc(object wordfile, object read_only, bool visible)
    {
        var objVisible = (object)visible;
        wrdApp.Visible = visible;
        var doc = wrdApp.Documents.Open(FileName: ref wordfile, ReadOnly: ref read_only, Visible: ref objVisible);
        doc.Activate();
        wrdApp.ActiveDocument.RunAutoMacro(Word.WdAutoMacros.wdAutoOpen);
        //if (visible) wrdApp.Activate();  //throws an exception if app is not visible
    }

    public void replace(string oldtext, string newtext, bool isRtf = false)
    {
        wrdApp.ActiveDocument.Select();
        // RTF needs to be pasted in
        if (isRtf)
        {
            Clipboard.Clear();
            Clipboard.SetText(newtext, TextDataFormat.Rtf);
            wrdApp.Selection.Find.Execute(oldtext, false, false, false, false, false, true, false, false, "^c", 2, false, false, false, false);
            Clipboard.Clear();
        }
        // This replace can only handle up to 255 characters
        else if (newtext.Length <= 255)
        {
            wrdApp.Selection.Find.Execute(oldtext, false, false, false, false, false, true, false, false, newtext, 2, false, false, false, false);
            object obj = wrdApp.ActiveDocument.Bookmarks[oEnd].Range;
            Word.Range r = (Word.Range)obj;
            r.Select();
        }
        else
        {
            wrdApp.Selection.Find.Execute(oldtext, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            wrdApp.Application.Selection.Text = newtext;
        }
    }

    public void replaceold(string oldtext, string newtext)
    {
        string text = wrdApp.ActiveDocument.Content.Text;
        object start = text.IndexOf(oldtext);
        if (start.ToString() == "-1")
            return;
        object end = (object)(int.Parse(start.ToString()) + oldtext.Length);
        wrdApp.ActiveDocument.Range(ref start, ref end).Text = newtext;
        //text = text.Replace(oldtext, newtext);
        //wrdApp.ActiveDocument.Range(ref oMissing, ref oMissing).Text = text.Substring(0,text.Length-1);
    }

    public void format(string oldtext, string function)
    {
        string text = wrdApp.ActiveDocument.Content.Text;
        object start = text.IndexOf(oldtext);
        if (start.ToString() == "-1")
            return;
        object end = (object)(int.Parse(start.ToString()) + oldtext.Length);
        Word.Range range = wrdApp.ActiveDocument.Range(ref start, ref end);
        if (function.IndexOf("b") != -1)
            range.Bold = 1;
        if (function.IndexOf("u") != -1)
            range.Underline = Word.WdUnderline.wdUnderlineSingle;
        if (function.IndexOf("4") != -1)
            range.Font.Size = 14;
    }

    public void setvisible(bool visible)
    {
        wrdApp.Visible = visible;
    }

    public void save(object path)
    {
        wrdApp.ActiveDocument.SaveAs(ref path, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
    }

    public void insertbreak(int? inchesfromtop = null)
    {
        // If inches from top is not specified, inserts the break at the end of the open document.
        if (inchesfromtop == null)
        {
            wrdApp.ActiveDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
        }
        else
        {
            double dPos = wrdApp.InchesToPoints(inchesfromtop.Value);
            object obj = wrdApp.ActiveDocument.Bookmarks[oEnd].Range;
            var r = (Word.Range)obj;
            object oPos = r.get_Information(Word.WdInformation.wdVerticalPositionRelativeToPage);
            if (dPos <= Convert.ToDouble(oPos) && Convert.ToDouble(oPos) > 2)
            {
                object pbreak = Word.WdBreakType.wdPageBreak;
                object oCollapseEnd = Word.WdCollapseDirection.wdCollapseEnd;
                r.Collapse(ref oCollapseEnd);
                r.InsertBreak(ref pbreak);
                r.Collapse(ref oCollapseEnd);
                r.InsertParagraphAfter();
            }
            else
            {
                r.InsertParagraphAfter();
            }
        }
    }

    public void insertNextPageBreak()
    {
        wrdApp.ActiveDocument.Words.Last.InsertBreak(Word.WdBreakType.wdSectionBreakNextPage);
    }

    public void inserttext(string text)
    {
        Word.Paragraph p;
        object obj = wrdApp.ActiveDocument.Bookmarks[oEnd].Range;
        p = wrdApp.ActiveDocument.Content.Paragraphs.Add(ref obj);
        if (text.IsRTF())
        {
            insertRTF(text, p.Range);
        }
        else
        {
            if (text.Contains("<b>"))
            {
                text = text.Replace("<b>", "");
                p.Range.Font.Bold = 2;
            }
            p.Range.Text = text;
            p.Format.SpaceAfter = 3;
            p.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            p.Range.InsertParagraphAfter();
        }
    }

    public void inserttable(DataTable dt, List<float> widths, bool blnShowGridLines = false)
    {
        var objRange = wrdApp.ActiveDocument.Bookmarks[oEnd].Range;
        var objTable = wrdApp.ActiveDocument.Tables.Add(objRange, dt.Rows.Count, dt.Columns.Count);
        objTable.Borders.InsideLineStyle = blnShowGridLines ? Word.WdLineStyle.wdLineStyleSingle : Word.WdLineStyle.wdLineStyleNone;
        objTable.Borders.OutsideLineStyle = blnShowGridLines ? Word.WdLineStyle.wdLineStyleSingle : Word.WdLineStyle.wdLineStyleNone;
        objTable.Range.ParagraphFormat.SpaceAfter = 3;

        for (int i = 0; i < widths.Count; i++)
            objTable.Columns[i + 1].Width = widths[i];

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            // The existence of these variables is due to the cae where one column of an inserted table is in RTF format, while subsequent
            // columns are not.  The font and font size are pulled from the RTF column, and applied to said subsequent columns.
            int? intFontSize = null;
            string strFont = null;

            for (int j = 0; j < dt.Columns.Count; j++)
            {
                var objCell = objTable.Cell(i + 1, j + 1);
                objCell.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                objRange = objCell.Range;
                string txt = dt.Rows[i][j].ToString();
                if (txt.Contains("<photo>"))
                {
                    txt = txt.Replace("<photo>", "");
                    string file = txt.Substring(0, txt.IndexOf("</>"));
                    object o = objRange;
                    objRange.Text = txt.Substring(txt.IndexOf("</>") + 3);
                    wrdApp.ActiveDocument.InlineShapes.AddPicture(file, ref oMissing, ref oMissing, ref o);
                }
                else if (txt.IsRTF())
                {
                    var rtfTextBox = new RichTextBox()
                    {
                        Text = txt
                    };
                    intFontSize = Convert.ToInt32(Math.Ceiling(rtfTextBox.SelectionFont.Size));
                    strFont = rtfTextBox.SelectionFont.Name;

                    insertRTF(txt, objRange);
                }
                else
                {
                    if (txt.Contains("<right>"))
                    {
                        txt = txt.Replace("<right>", "");
                        objRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    }
                    else
                    {
                        objRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    }

                    objRange.Text = txt;

                    // Only change the font and font size used if a previous column was in RTF format
                    if (!String.IsNullOrEmpty(strFont))
                        objRange.Font.Name = strFont;

                    if (intFontSize.HasValue)
                        objRange.Font.Size = intFontSize.Value;
                }
            }
        }
    }

    /// <summary>
    ///     Copies the contents of the given document to the clipboard.  If the paste variable 
    ///     is true, this method will append the current contents of the clipboard to the end 
    ///     of the given document prior to the copying to the clipboard.
    /// </summary>
    public void copydoc(string doc, bool paste, float? fontSize = null)
    {
        if (Path.GetExtension(doc).EqualsIgnoreCase(".pdf"))
        {
            createworddoc("", false);
            string imagePath = Path.GetTempPath() + Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".jpg";
            using (var document = PdfDocument.Load(doc))
            {
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    const int dpi = 150;
                    using (var image = document.Render(1 - 1, 1200, 1650, dpi, dpi, PdfRenderFlags.Annotations))
                    {
                        image.Save(stream, ImageFormat.Jpeg);
                    }
                }
            }

            wrdApp.ActiveDocument.Bookmarks[oEnd].Range.InlineShapes.AddPicture(imagePath,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value);
        }
        else
        {
            openworddoc(doc, true, false, false, fontSize);
        }
        if (paste)
        {
            insertbreak(1);
            object obj = wrdApp.ActiveDocument.Bookmarks[oEnd].Range;
            Word.Range r = (Word.Range)obj;
            r.Select();
            wrdApp.ActiveWindow.Selection.Paste();
        }
        wrdApp.ActiveWindow.Selection.WholeStory();
        if (fontSize != null)
            wrdApp.ActiveWindow.Selection.Font.Size = (float)fontSize;
        wrdApp.ActiveWindow.Selection.Copy();
        wrdApp.ActiveDocument.Close(ref oFalse);
    }

    public void insertRTF(string strRTF, Word.Range objRange = null, int? intFontSize = null)
    {
        if (strRTF.IsNullOrBlank())
            return;

        Retry.Do(() =>
        {
            IDataObject formatedText = null;
            Thread thread = new Thread(delegate()
            {
                Clipboard.Clear();
                Clipboard.SetText(strRTF, TextDataFormat.Rtf);
                formatedText = Clipboard.GetDataObject();
            });
            thread.SetApartmentState(ApartmentState.STA); // Set the thread to STA.
            thread.Start();
            thread.Join(); // Wait for the thread to end.

            if (formatedText != null)
            {
                if (objRange != null)
                {
                    objRange.Select();
                    objRange.PasteSpecial(DataType: Word.WdPasteDataType.wdPasteRTF);
                }
                else
                {
                    Word._Document wrdDoc = wrdApp.Documents.Add();
                    wrdApp.ActiveWindow.Selection.Paste();
                    if (intFontSize != null)
                    {
                        var range = wrdDoc.Range(wrdDoc.Content.Start, wrdDoc.Content.End);
                        range.Font.Size = (int)intFontSize;
                    }
                }
            }

            thread = new Thread(delegate ()
            {
                Clipboard.Clear();
            });
            thread.SetApartmentState(ApartmentState.STA); // Set the thread to STA.
            thread.Start();
            thread.Join(); // Wait for the thread to end.
        }
        );
    }

    /// <summary>
    /// This method was pulled from stack overflow:
    /// https://stackoverflow.com/questions/607669/
    /// </summary>
    public string WordToPDF(string strFileToConvert)
    {
        wrdApp.Visible = false;
        wrdApp.ScreenUpdating = false;

        // C# doesn't have optional arguments so we'll need a dummy value
        var objVisible = (object)false;
        var objReadyOnly = (object)true;

        // Get list of Word files in specified directory
        var wordFile = new FileInfo(strFileToConvert);

        // Cast as Object for word Open method
        var filename = (object)wordFile.FullName;

        // Use the dummy value as a placeholder for optional arguments
        var doc = wrdApp.Documents.Open(ref filename, Visible: ref objVisible, ReadOnly: ref objReadyOnly);
        doc.Activate();

        var outputFileName = (object)wordFile.FullName.Replace(".docx", ".pdf").Replace(".doc", ".pdf").Replace(".rtf", ".pdf");
        var fileFormat = (object)Word.WdSaveFormat.wdFormatPDF;

        // Save document into PDF Format
        doc.SaveAs(ref outputFileName, ref fileFormat);

        // Close the Word document, but leave the Word application open.
        // doc has to be cast to type _Document so that it will find the
        // correct Close method.                
        var saveChanges = (object)Word.WdSaveOptions.wdDoNotSaveChanges;
        doc.Close(ref saveChanges);

        wrdApp.Quit();

        return outputFileName.ToString();
    }

    #region print

    public void printworddoc(string printerName = "", bool includePageNumbers = false, string prePageNumberText = "", string postPageNumberText = "", string pdfPath = "")
    {
        string orignalPrinter = "";
        if (printerName != "")
        {
            //This changes the default printer on the computer so we will want to set it back after we print our document.
            orignalPrinter = wrdApp.ActivePrinter;
            wrdApp.ActivePrinter = printerName;
        }
        if (includePageNumbers)
        {
            //Pulled from https://social.msdn.microsoft.com/Forums/vstudio/en-US/a044ff2d-b4a7-4f19-84f4-f3d5c55396a8/insert-current-page-number-page-x-of-n-on-a-word-document?forum=vsto
            // Open up the footer in the word document
            wrdApp.ActiveWindow.ActivePane.View.SeekView = Word.WdSeekView.wdSeekCurrentPageFooter;
            // Set current Paragraph Alignment to Center
            wrdApp.ActiveWindow.ActivePane.Selection.Paragraphs.Alignment = Word.WdParagraphAlignment
                .wdAlignParagraphCenter;
            if(!prePageNumberText.IsNullOrBlank())
                wrdApp.ActiveWindow.Selection.TypeText(prePageNumberText);
            // Type in 'Page '
            wrdApp.ActiveWindow.Selection.TypeText("Page ");
            // Add in current page field
            Object CurrentPage = Word.WdFieldType.wdFieldPage;
            wrdApp.ActiveWindow.Selection.Fields.Add(wrdApp.ActiveWindow.Selection.Range, ref CurrentPage, ref oMissing, ref oMissing);
            // Type in ' of '
            wrdApp.ActiveWindow.Selection.TypeText(" of ");
            // Add in total page field
            Object TotalPages = Word.WdFieldType.wdFieldNumPages;
            wrdApp.ActiveWindow.Selection.Fields.Add(wrdApp.ActiveWindow.Selection.Range, ref TotalPages, ref oMissing, ref oMissing);
            if (!string.IsNullOrEmpty(postPageNumberText))
                wrdApp.ActiveWindow.Selection.TypeText(postPageNumberText);
        }
        if (!string.IsNullOrEmpty(pdfPath))
        {
            wrdApp.ActiveDocument.SaveAs(pdfPath, Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        }
        else
        {
            wrdApp.PrintOut(ref oFalse, ref oTrue, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            Thread.Sleep(500); //Small pause to let word finish printing
        }
        if (printerName != null)
            wrdApp.ActivePrinter = orignalPrinter;
    }
    #endregion

    #region public void mailmerge(object doc, string datafile)
    public void mailmerge(object doc, string datafile)
    {
        Word.Application wrdApp;
        Word._Document wrdDoc;
        Object oMissing = new object();
        oMissing = System.Reflection.Missing.Value;
        Object oFalse = new object();
        oFalse = false;
        Word.Selection wrdSelection;
        Word.MailMerge wrdMailMerge;
        wrdApp = new Word.Application();
        wrdDoc = wrdApp.Documents.Open(ref doc, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        wrdDoc.Select();
        wrdSelection = wrdApp.Selection;
        wrdMailMerge = wrdDoc.MailMerge;
        wrdMailMerge.OpenDataSource(datafile, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        wrdMailMerge.Destination = Word.WdMailMergeDestination.wdSendToNewDocument;
        wrdMailMerge.Execute(ref oFalse);
        wrdDoc.Saved = true;
        wrdDoc.Close(ref oFalse, ref oMissing, ref oMissing);
        wrdApp.Visible = true;
        wrdSelection = null;
        wrdMailMerge = null;
        wrdDoc = null;
        wrdApp = null;
    }
    #endregion

    #region public void runmacro(object oApp, object[] oRunArgs)
    public void runmacro(object oApp, object[] oRunArgs)
    {
        oApp.GetType().InvokeMember("Run", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod, null, oApp, oRunArgs);
    }
    #endregion

    #region public Word.Application quitword( Word.Application app, bool SaveChanges )
    public Word.Application quitword(Word.Application app, bool SaveChanges)
    {
        object sc = SaveChanges;
        foreach (Word.Document doc in app.Documents)
            doc.Close(ref sc, ref oMissing, ref oMissing);
        app.Quit(ref sc, ref oMissing, ref oMissing);
        return app;
    }
    #endregion

    #region public void quitword(  )
    public void quitword()
    {
        quitword(wrdApp, false);
    }
    #endregion

    #region public void quitsave( Word.Application app )
    public void quitsave()
    {
        quitword(wrdApp, true);
    }
    #endregion
}
