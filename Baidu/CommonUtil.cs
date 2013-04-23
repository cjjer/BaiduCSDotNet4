using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudAPI.Baidu
{
    public static class Vars
    {
        public const string BCS_SDK_ACL_TYPE_PUBLIC_READ = "public-read";
        //公开写权限（不具备删除权限）

        public const string BCS_SDK_ACL_TYPE_PUBLIC_WRITE = "public-write";
        //公开读写权限（不具备删除权限）

        public const string BCS_SDK_ACL_TYPE_PUBLIC_READ_WRITE = "public-read-write";
        //公开所有权限

        public const string BCS_SDK_ACL_TYPE_PUBLIC_CONTROL = "public-control";
        //私有权限，仅bucket所有者具有所有权限

        public const string BCS_SDK_ACL_TYPE_PRIVATE = "private";


        public const string Acl = "x-bs-acl";
        public const string MetaKey = "x-bs-meta-key";
        public const string RenameType = "renametype";
        public const string RenameType_Md5 = "md5";

        //Action 用户动作

        //'*'代表所有action 
        public const string BCS_SDK_ACL_ACTION_ALL = "*";
        //与bucket相关的action
        public const string BCS_SDK_ACL_ACTION_LIST_OBJECT = "list_object";
        public const string BCS_SDK_ACL_ACTION_PUT_BUCKET_POLICY = "put_bucket_policy";
        public const string BCS_SDK_ACL_ACTION_GET_BUCKET_POLICY = "get_bucket_policy";
        public const string BCS_SDK_ACL_ACTION_DELETE_BUCKET = "delete_bucket";
        //与object相关的action
        public const string BCS_SDK_ACL_ACTION_GET_OBJECT = "get_object";
        public const string BCS_SDK_ACL_ACTION_PUT_OBJECT = "put_object";
        public const string BCS_SDK_ACL_ACTION_DELETE_OBJECT = "delete_object";
        public const string BCS_SDK_ACL_ACTION_PUT_OBJECT_POLICY = "put_object_policy";
        public const string BCS_SDK_ACL_ACTION_GET_OBJECT_POLICY = "get_object_policy";
        //EFFECT:
        public const string BCS_SDK_ACL_EFFECT_ALLOW = "allow";
        public const string BCS_SDK_ACL_EFFECT_DENY = "deny";

    }
    public static class CommonUtil
    {

        static Dictionary<string, string> a = null;
        public static string GetMimeTypes(string ext)
        {
            if (String.IsNullOrWhiteSpace(ext)) return null;
            ext = ext.ToLower().Trim();
            if (ext.StartsWith(".")) ext = ext.Substring(1);
            if (a.ContainsKey(ext)) return a[ext];
            return null;
        }
        static CommonUtil()
        {
            a = new Dictionary<string, string>();
            a.Add("3gp", "video/3gpp");
            a.Add("ai", "application/postscript");
            a.Add("aif", "audio/x-aiff");
            a.Add("aifc", "audio/x-aiff");
            a.Add("aiff", "audio/x-aiff");
            a.Add("asc", "text/plain");
            a.Add("atom", "application/atom+xml");
            a.Add("au", "audio/basic");
            a.Add("avi", "video/x-msvideo");
            a.Add("bcpio", "application/x-bcpio");
            a.Add("bin", "application/octet-stream");
            a.Add("bmp", "image/bmp");
            a.Add("cdf", "application/x-netcdf");
            a.Add("cgm", "image/cgm");
            a.Add("class", "application/octet-stream");
            a.Add("cpio", "application/x-cpio");
            a.Add("cpt", "application/mac-compactpro");
            a.Add("csh", "application/x-csh");
            a.Add("css", "text/css");
            a.Add("dcr", "application/x-director");
            a.Add("dif", "video/x-dv");
            a.Add("dir", "application/x-director");
            a.Add("djv", "image/vnd.djvu");
            a.Add("djvu", "image/vnd.djvu");
            a.Add("dll", "application/octet-stream");
            a.Add("dmg", "application/octet-stream");
            a.Add("dms", "application/octet-stream");
            a.Add("doc", "application/msword");
            a.Add("dtd", "application/xml-dtd");
            a.Add("dv", "video/x-dv");
            a.Add("dvi", "application/x-dvi");
            a.Add("dxr", "application/x-director");
            a.Add("eps", "application/postscript");
            a.Add("etx", "text/x-setext");
            a.Add("exe", "application/octet-stream");
            a.Add("ez", "application/andrew-inset");
            a.Add("flv", "video/x-flv");
            a.Add("gif", "image/gif");
            a.Add("gram", "application/srgs");
            a.Add("grxml", "application/srgs+xml");
            a.Add("gtar", "application/x-gtar");
            a.Add("gz", "application/x-gzip");
            a.Add("hdf", "application/x-hdf");
            a.Add("hqx", "application/mac-binhex40");
            a.Add("htm", "text/html");
            a.Add("html", "text/html");
            a.Add("ice", "x-conference/x-cooltalk");
            a.Add("ico", "image/x-icon");
            a.Add("ics", "text/calendar");
            a.Add("ief", "image/ief");
            a.Add("ifb", "text/calendar");
            a.Add("iges", "model/iges");
            a.Add("igs", "model/iges");
            a.Add("jnlp", "application/x-java-jnlp-file");
            a.Add("jp2", "image/jp2");
            a.Add("jpe", "image/jpeg");
            a.Add("jpeg", "image/jpeg");
            a.Add("jpg", "image/jpeg");
            a.Add("js", "application/x-javascript");
            a.Add("kar", "audio/midi");
            a.Add("latex", "application/x-latex");
            a.Add("lha", "application/octet-stream");
            a.Add("lzh", "application/octet-stream");
            a.Add("m3u", "audio/x-mpegurl");
            a.Add("m4a", "audio/mp4a-latm");
            a.Add("m4p", "audio/mp4a-latm");
            a.Add("m4u", "video/vnd.mpegurl");
            a.Add("m4v", "video/x-m4v");
            a.Add("mac", "image/x-macpaint");
            a.Add("man", "application/x-troff-man");
            a.Add("mathml", "application/mathml+xml");
            a.Add("me", "application/x-troff-me");
            a.Add("mesh", "model/mesh");
            a.Add("mid", "audio/midi");
            a.Add("midi", "audio/midi");
            a.Add("mif", "application/vnd.mif");
            a.Add("mov", "video/quicktime");
            a.Add("movie", "video/x-sgi-movie");
            a.Add("mp2", "audio/mpeg");
            a.Add("mp3", "audio/mpeg");
            a.Add("mp4", "video/mp4");
            a.Add("mpe", "video/mpeg");
            a.Add("mpeg", "video/mpeg");
            a.Add("mpg", "video/mpeg");
            a.Add("mpga", "audio/mpeg");
            a.Add("ms", "application/x-troff-ms");
            a.Add("msh", "model/mesh");
            a.Add("mxu", "video/vnd.mpegurl");
            a.Add("nc", "application/x-netcdf");
            a.Add("oda", "application/oda");
            a.Add("ogg", "application/ogg");
            a.Add("ogv", "video/ogv");
            a.Add("pbm", "image/x-portable-bitmap");
            a.Add("pct", "image/pict");
            a.Add("pdb", "chemical/x-pdb");
            a.Add("pdf", "application/pdf");
            a.Add("pgm", "image/x-portable-graymap");
            a.Add("pgn", "application/x-chess-pgn");
            a.Add("pic", "image/pict");
            a.Add("pict", "image/pict");
            a.Add("png", "image/png");
            a.Add("pnm", "image/x-portable-anymap");
            a.Add("pnt", "image/x-macpaint");
            a.Add("pntg", "image/x-macpaint");
            a.Add("ppm", "image/x-portable-pixmap");
            a.Add("ppt", "application/vnd.ms-powerpoint");
            a.Add("ps", "application/postscript");
            a.Add("qt", "video/quicktime");
            a.Add("qti", "image/x-quicktime");
            a.Add("qtif", "image/x-quicktime");
            a.Add("ra", "audio/x-pn-realaudio");
            a.Add("ram", "audio/x-pn-realaudio");
            a.Add("ras", "image/x-cmu-raster");
            a.Add("rdf", "application/rdf+xml");
            a.Add("rgb", "image/x-rgb");
            a.Add("rm", "application/vnd.rn-realmedia");
            a.Add("roff", "application/x-troff");
            a.Add("rtf", "text/rtf");
            a.Add("rtx", "text/richtext");
            a.Add("sgm", "text/sgml");
            a.Add("sgml", "text/sgml");
            a.Add("sh", "application/x-sh");
            a.Add("shar", "application/x-shar");
            a.Add("silo", "model/mesh");
            a.Add("sit", "application/x-stuffit");
            a.Add("skd", "application/x-koan");
            a.Add("skm", "application/x-koan");
            a.Add("skp", "application/x-koan");
            a.Add("skt", "application/x-koan");
            a.Add("smi", "application/smil");
            a.Add("smil", "application/smil");
            a.Add("snd", "audio/basic");
            a.Add("so", "application/octet-stream");
            a.Add("spl", "application/x-futuresplash");
            a.Add("src", "application/x-wais-source");
            a.Add("sv4cpio", "application/x-sv4cpio");
            a.Add("sv4crc", "application/x-sv4crc");
            a.Add("svg", "image/svg+xml");
            a.Add("swf", "application/x-shockwave-flash");
            a.Add("t", "application/x-troff");
            a.Add("tar", "application/x-tar");
            a.Add("tcl", "application/x-tcl");
            a.Add("tex", "application/x-tex");
            a.Add("texi", "application/x-texinfo");
            a.Add("texinfo", "application/x-texinfo");
            a.Add("tif", "image/tiff");
            a.Add("tiff", "image/tiff");
            a.Add("tr", "application/x-troff");
            a.Add("tsv", "text/tab-separated-values");
            a.Add("txt", "text/plain");
            a.Add("ustar", "application/x-ustar");
            a.Add("vcd", "application/x-cdlink");
            a.Add("vrml", "model/vrml");
            a.Add("vxml", "application/voicexml+xml");
            a.Add("wav", "audio/x-wav");
            a.Add("wbmp", "image/vnd.wap.wbmp");
            a.Add("wbxml", "application/vnd.wap.wbxml");
            a.Add("webm", "video/webm");
            a.Add("wml", "text/vnd.wap.wml");
            a.Add("wmlc", "application/vnd.wap.wmlc");
            a.Add("wmls", "text/vnd.wap.wmlscript");
            a.Add("wmlsc", "application/vnd.wap.wmlscriptc");
            a.Add("wmv", "video/x-ms-wmv");
            a.Add("wrl", "model/vrml");
            a.Add("xbm", "image/x-xbitmap");
            a.Add("xht", "application/xhtml+xml");
            a.Add("xhtml", "application/xhtml+xml");
            a.Add("xls", "application/vnd.ms-excel");
            a.Add("xml", "application/xml");
            a.Add("xpm", "image/x-xpixmap");
            a.Add("xsl", "application/xml");
            a.Add("xslt", "application/xslt+xml");
            a.Add("xul", "application/vnd.mozilla.xul+xml");
            a.Add("xwd", "image/x-xwindowdump");
            a.Add("xyz", "chemical/x-xyz");
            a.Add("apk", "application/vnd.android.package-archive");
            a.Add("cab", "application/vnd.ms-cab-compressed");
            a.Add("gb", "application/chinese-gb");
            a.Add("gba", "application/octet-stream");
            a.Add("gbc", "application/octet-stream");
            a.Add("jad", "text/vnd.sun.j2me.app-descriptor");
            a.Add("jar", "application/java-archive");
            a.Add("nes", "application/octet-stream");
            a.Add("rar", "application/x-rar-compressed");
            a.Add("sis", "application/vnd.symbian.install");
            a.Add("sisx", "x-epoc/x-sisx-app");
            a.Add("smc", "application/octet-stream");
            a.Add("smd", "application/octet-stream");
            a.Add("zip", "application/x-zip-compressed");
            a.Add("wap", "text/vnd.wap.wml");
            a.Add("wma", "audio/x-ms-wma");
        }
    }

}
