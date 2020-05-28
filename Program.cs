using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GetDirectoryCount
{
    class Program
    {
        //所有文件总数
        static int fileCount = 0;

        //空目录总数
        static int emptyDirectoryCount = 0;

        //计时器
        static Stopwatch stopwatch = new Stopwatch();

        //所有文件总数对象锁
        private readonly static object lockFileCountObject = new object();

        //空目录总数对象锁
        private readonly static object lockEmptyDirectoryCountObject = new object();

        static void Main(string[] args)
        {
            GetCount();
        }

        static void GetCount()
        {
            fileCount = 0;
            emptyDirectoryCount = 0;

            Console.WriteLine("请输入目录:");
            string path = Console.ReadLine();

            if (!Directory.Exists(path))
            {
                Console.WriteLine("目录不存在！");
                GetCount();
                return;
            }

            //重置计时
            stopwatch.Reset();

            //开始计时
            stopwatch.Start();

            //当前输入的目录信息
            DirectoryInfo root = new DirectoryInfo(path);

            //任务列表
            List<Task> taskList = new List<Task>();

            //获取当前输入的目录的文件数量
            var task = Task.Run(() => { IncEmptyDirectoryCount(GetFileCount(root)); });
            taskList.Add(task);

            //获取子目录
            var childs = root.GetDirectories().Where(s => s.Name != "System Volume Information").ToArray();

            //遍历子目录
            for (int i = 0; i < childs.Length; i++)
            {
                var child = childs[i];
                task = Task.Run(() => GetDirectoryCount(child));
                taskList.Add(task);
            }

            //等待完成
            Task.WaitAll(taskList.ToArray());

            //停止计时
            stopwatch.Stop();

            //获取耗时
            TimeSpan timeSpan = stopwatch.Elapsed;

            //时间转换
            DateTime time = ConvertToDateTime(timeSpan);

            Console.WriteLine($"运行框架：{RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"操作系统：{RuntimeInformation.OSDescription}");
            Console.WriteLine($"平台架构：{RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"执行耗时：{time.ToString("HH:mm:ss.fff")}");
            Console.WriteLine($"所有文件数量：{fileCount}");
            Console.WriteLine($"空文件夹个数：{emptyDirectoryCount}");

            //重新开始
            GetCount();
        }

        /// <summary>
        /// 获取目录下的文件数量
        /// </summary>
        /// <param name="pathInfo">目录信息</param>
        static int GetFileCount(DirectoryInfo pathInfo)
        {
            return pathInfo.GetFiles().Length;
        }

        /// <summary>
        /// 遍历子目录下的文件数量和空文件夹数量
        /// </summary>
        /// <param name="pathInfo">目录信息</param>
        /// <returns></returns>
        static void GetDirectoryCount(DirectoryInfo pathInfo)
        {
            //获取当前目录下的文件数量
            int count = GetFileCount(pathInfo);

            //所有文件数量递增
            IncFileCount(count);

            //获取当前目录下的子目录数量
            var pathInfos = pathInfo.GetDirectories();
            for(int i = 0; i < pathInfos.Length; i++)            
            {
                //递归获取子目录
                GetDirectoryCount(pathInfos[i]);
            }

            //如果文件数量和子目录数量为0
            if (count == 0 && pathInfos.Length == 0)
            {
                //空目录数量递增1
                IncEmptyDirectoryCount(1);
            }
        }

        /// <summary>
        /// 时间戳转换为日志
        /// </summary>
        /// <param name="timeSpan">时间戳</param>
        /// <returns></returns>
        static DateTime ConvertToDateTime(TimeSpan timeSpan)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddMilliseconds(timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// 递增所有文件总数
        /// </summary>
        /// <param name="num">数量</param>
        static void IncFileCount(int num)
        {
            lock(lockFileCountObject)
            {
                fileCount += num;
            }
        }

        /// <summary>
        /// 递增空目录总数
        /// </summary>
        /// <param name="num">数量</param>
        static void IncEmptyDirectoryCount(int num)
        {
            lock (lockEmptyDirectoryCountObject)
            {
                emptyDirectoryCount += num;
            }
        }
    }
}
