using System;
using System.Collections.Generic;
using System.IO;
using iText.Kernel.Pdf;
using System.Xml;
using Microsoft.Extensions.Logging;
namespace Ghostscript.NET.FacturX.ZUGFeRD
{

    public class ZUGFeRDExporter
    {
        protected string? gsDLL = null;
        protected string? sourcePDF = null;
        protected bool noSourceCopy = false;
        protected string profile = "EN16931";
        protected int version = 2;
        protected IExportableTransaction? trans = null;

        public ZUGFeRDExporter(string gsDLL)
        {
            this.gsDLL = gsDLL;
        }
        public ZUGFeRDExporter load(string PDFfilename)
        {
            string basename=Path.GetFileName(PDFfilename);
            this.sourcePDF = Path.GetTempPath() + basename;
            string d1=Path.GetDirectoryName(PDFfilename)+Path.DirectorySeparatorChar;
            string d2=Path.GetTempPath();
            if (d1.Equals(Path.GetTempPath())) {
                noSourceCopy=true;
            } else {
                File.Copy(PDFfilename, sourcePDF);
            }
            return this;
        }
        public ZUGFeRDExporter setTransaction(IExportableTransaction trans)
        {
            this.trans = trans;
            return this;
        }

        public ZUGFeRDExporter setZUGFeRDVersion(int version)
        {
            this.version = version;
            return this;
        }

        public ZUGFeRDExporter setProfile(string profile)
        {
            this.profile = profile;
            return this;
        }


        public void export(string targetFilename)
        {

            PDFConverter pc = new PDFConverter(sourcePDF, targetFilename);

            ZUGFeRD2PullProvider zf2p = new ZUGFeRD2PullProvider();
            zf2p.setProfile(Profiles.getByName(profile));
            zf2p.generateXML(trans);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            string tempfilename = Path.GetTempPath() + "\\factur-x.xml";
            File.WriteAllBytes(tempfilename, zf2p.getXML());

            pc.EmbedXMLForZF(tempfilename, Convert.ToString(version));
            pc.ConvertToPDFA3(gsDLL);
            File.Delete(Path.GetTempPath() + "\\factur-x.xml");
            if (!noSourceCopy) {
                File.Delete(sourcePDF);
            }

        }
    }
}
