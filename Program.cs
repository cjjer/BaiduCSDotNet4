using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CloudAPI.Baidu.Model;
using CloudAPI.Baidu;
using System.IO;
using System.Net.Http.Headers;

namespace CloudAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var _AccessKey = "BEfd4eac2da6f443f000c3d02e366adc";
            var _SecureKey = "EC2dae855ae625ef0000c5c0f4103992";
            Baidu.CoreClient api = new Baidu.CoreClient(new Uri("http://bcs.duapp.com/"), _AccessKey, _SecureKey);
            string tempkey = Regex.Replace(_AccessKey + _SecureKey, @"\d", "").ToLower();
            Console.WriteLine("创建{1}结果为：{0}", api.CreateBucket(tempkey), tempkey);
            Console.WriteLine("删除{1}结果为：{0}", api.DeleteBucket(tempkey), tempkey);
            var ls = api.GetBucketList();
            if (ls != null)
            {

                foreach (var ki in ls)
                {
                    Console.WriteLine("名称:{0},状态：{1}", ki.Name, ki.Status);
                }
            }
            //Console.WriteLine("删除结果为：{0}", api.DeleteBucket("sphere0001"));
            string bucket = "nanjing";
            string objectId = "/my" + DateTime.Now.Ticks.ToString() + ".pdf";

            BCSObjectHead head = new BCSObjectHead();
            head.Acl = Vars.BCS_SDK_ACL_TYPE_PUBLIC_READ;
            head.ContentType = MediaTypeHeaderValue.Parse(CommonUtil.GetMimeTypes(Path.GetExtension(objectId)));
            head.Expires = DateTime.Now.AddDays(5d);
            head.MetaKey = "version=v1";
            head.FilePath = "昆明制药.PDF";
            //head.FileName = head.FilePath + "哈哈哈." + ".png";

            var createResult = api.CreateObject(bucket, objectId, head);

            string objectid = "/my635023078156562500.pdf";
            Console.WriteLine("DeleteObject{1}结果为：{0}", api.DeleteObject(bucket, objectid), objectid);

            var objectls = api.GetObjectList(bucket,0,100);
            if (objectls != null)
            {
                if (objectls.Items != null)
                {
                    foreach (var ki in objectls.Items)
                    {
                        api.DeleteObject(bucket, ki.Id);
                        Console.WriteLine("名称:{0}", ki.Path);
                    }
                }
            }
            var getOne = api.GetObject(bucket, objectId, "新的文件.pdf");
            Console.WriteLine("getOne{1}结果为：{0}", getOne, objectId);


            //var aclOne = api.GetAcl(bucket, objectId);
            //if (aclOne != null)
            //{
            //    Console.WriteLine("获取到的Acl为{0}", aclOne.Text);
            //}
            //aclOne.Text[0].ResourceList = new string[]{"shanghai/*"};

            //var aclSend = new AclModel();
            //aclSend.Text = new List<AclQuery>();

            //var aclSend = api.GetAcl("shanghai", "/test.txt");
            //var aclSendOne = new AclQuery();
            //aclSendOne.ActionText = new string[] { Vars.BCS_SDK_ACL_ACTION_GET_OBJECT };
            //aclSendOne.Effect = Vars.BCS_SDK_ACL_EFFECT_ALLOW;
            //aclSendOne.ResourceList = new string[] { "shanghai/test.txt" };
            //aclSendOne.UserList = new string[] { "*" };
            //aclSend.Text.Clear();
            //aclSend.Text.Add(aclSendOne);

            //api.PutAcl("shanghai", "/test.txt", aclSend);
            //api.PutAcl("shanghai", "/test.txt", null );
            //aclOne = api.GetAcl("shanghai", "/test.txt");
            //aclOne.Text[0].ActionText = new string[] { Vars.BCS_SDK_ACL_ACTION_ALL,Vars.BCS_SDK_ACL_ACTION_LIST_OBJECT};
            //Console.WriteLine("提交Acl的结果为为{0}", api.PutAcl(bucket, objectId, aclOne)); ;
            //aclOne = api.GetAcl(bucket, objectId);
            //if (aclOne != null)
            //{
            //    Console.WriteLine("获取到的Acl为{0}", aclOne.Text);
            //}
        }
    }
}
