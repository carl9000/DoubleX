﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using NPOI;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Linq;

namespace UTH.Infrastructure.Utility
{
    /// <summary>
    /// 文件操作辅助类
    /// </summary>
    public class FilesHelper
    {
        #region 路径

        /// <summary>
        /// 获取文件路径($"{dir}/{name}")
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        /// <param name="isCreateDir"></param>
        /// <returns></returns>
        public static string GetFilePath(string dir, string name, bool isCreateDir = true)
        {
            if (!Directory.Exists(dir) && isCreateDir)
            {
                Directory.CreateDirectory(dir);
            }
            return $"{dir}/{name}";
        }

        /// <summary>
        /// 获取md5目录路径
        /// </summary>
        /// <param name="root"></param>
        /// <param name="md5"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMd5DirPath(string root, string md5, string path = "")
        {
            md5.CheckEmpty();
            if (md5.Length < 4)
                throw new ArgumentException(nameof(md5));

            return $"{root}{path}/{md5.Substring(0, 1)}/{md5.Substring(1, 1)}/{md5.Substring(2, 2)}";
        }

        /// <summary>
        /// 获取md5文件名
        /// </summary>
        /// <param name="md5"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetMd5FileName(string md5, string name)
        {
            return $"{md5}{Path.GetExtension(name)}";
        }

        #endregion

        #region 目录/文件

        /// <summary>
        /// 获取目录信息(不存在返回null)
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="isExistCreate">不存在是否创建</param>
        /// <returns></returns>
        public static DirectoryInfo GetDirectory(string path, bool isExistCreate = true)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists && isExistCreate)
            {
                dir.Create();
                dir = new DirectoryInfo(path);
            }
            return dir;
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="isExistCreate"></param>
        /// <returns></returns>
        public static FileInfo GetFile(string path, string name, bool isExistCreate = true)
        {
            return GetFile(GetDirectory(path, isExistCreate: isExistCreate), name);
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FileInfo GetFile(DirectoryInfo directory, string name)
        {
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException();
            }
            name.CheckEmpty();
            return new FileInfo($"{directory.FullName}/{name}");
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileInfo GetFile(string filePath)
        {
            return new FileInfo(filePath);
        }

        #endregion

        #region 保存

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path">保存目录</param>
        /// <param name="name">文件名</param>
        /// <param name="content">内容 </param>
        public static void SaveFile(string path, string name, string content)
        {
            FileStream fileStream = null;
            StreamWriter writeStream = null;
            try
            {
                CreateFold(path, isFilePath: false);
                string filePath = GetPath(path, name);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                writeStream = new StreamWriter(fileStream);
                writeStream.WriteLine(content);
                writeStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
                if (writeStream != null)
                {
                    writeStream.Close();
                    writeStream.Dispose();
                }
            }
        }

        /// <summary>
        /// 保存文件(字节数组)
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="isAppend"></param>
        /// <param name="isExistDelete"></param>
        /// <returns></returns>
        public static FileInfo SaveFile(string dir, string name, byte[] bytes, int offset = 0, bool isAppend = false, bool isExistDelete = true)
        {
            FileStream fileStream = null;
            try
            {
                var filePath = GetFilePath(dir, name);
                if (File.Exists(filePath))
                {
                    if (!isExistDelete)
                    {
                        return GetFile(filePath);
                    }
                    File.Delete(filePath);
                }

                if (isAppend)
                {
                    fileStream = new FileStream(filePath, FileMode.Append, FileAccess.ReadWrite);
                }
                else
                {
                    fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                }

                fileStream.Write(bytes, offset, bytes.Length);
                return GetFile(filePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
        }

        /// <summary>
        /// 保存分片
        /// </summary>
        /// <param name="dir">保存目录</param>
        /// <param name="name">文件名称</param>
        /// <param name="chunk">当前分片 从0开始，eg:如总6个，则当前值 0,1,2,3,4,5</param>
        /// <param name="chunks">分片总数</param>
        /// <param name="bytes">字节内容</param>
        /// <param name="isExistDelete">存在是否删除</param>
        /// <returns></returns>
        public static FileInfo SaveChunks(string dir, string name, long chunk, long chunks, byte[] bytes, bool isExistDelete = true)
        {
            FileStream chunkStream = null;
            try
            {
                var chunkPath = GetFilePath(dir, $"{Path.GetFileNameWithoutExtension(name)}_{chunk}");
                if (File.Exists(chunkPath))
                {
                    if (!isExistDelete)
                    {
                        return GetFile(chunkPath);
                    }
                    File.Delete(chunkPath);
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                chunkStream = new FileStream(chunkPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                chunkStream.Write(bytes, 0, bytes.Length);
                return GetFile(chunkPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (chunkStream != null)
                {
                    chunkStream.Close();
                    chunkStream.Dispose();
                }
            }
        }

        /// <summary>
        /// 合并分片
        /// </summary>
        /// <param name="chunkDir"></param>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        /// <param name="isExistDelete"></param>
        /// <returns></returns>
        public static FileInfo MergeChunks(string chunkDir, string dir, string name, bool isExistDelete = true)
        {
            FileStream fileStream = null;
            try
            {
                var filePath = GetFilePath(dir, name);

                var chunks = Directory.GetFiles(chunkDir).ToList().OrderBy(x => x.Length).OrderBy(x => x);
                if (chunks.Count() == 0)
                {
                    return GetFile(filePath);
                }

                if (File.Exists(filePath))
                {
                    if (!isExistDelete)
                    {
                        return GetFile(filePath);
                    }
                    File.Delete(filePath);
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                foreach (var part in chunks)
                {
                    var chunkBytes = File.ReadAllBytes(part);
                    fileStream.Write(chunkBytes, 0, chunkBytes.Length);
                    chunkBytes = null;
                    File.Delete(part);
                }
                Directory.Delete(chunkDir);
                return GetFile(filePath);
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
        }

        #endregion

        #region 文件下载

        /// <summary>
        /// 客户端HTTP异步方式下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="post"></param>
        /// <param name="head"></param>
        /// <param name="path"></param>
        /// <param name="action"></param>
        public static async void DownloadByHttpAsync(string url, string post, string head, string path, Action<long, int> action)
        {
            await Task.Run(async () =>
            {
                using (HttpClient http = new HttpClient())
                {
                    long downSize = 0;  //已经下载大小
                    long downSpeed = 0; //下载速度

                    var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    var repLength = response.Content.Headers.ContentLength;
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var size = 1024000;                                             //1000K
                        byte[] bytes = new byte[size];

                        int writes;
                        var beginSecond = DateTime.Now.Second;                          //当前时间秒

                        while ((writes = stream.Read(bytes, 0, size)) > 0)
                        {
                            //使用追加方式打开一个文件流
                            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                            {
                                fs.Write(bytes, 0, writes);
                            }

                            downSize += writes;
                            downSpeed += writes;

                            //计算下载速度kb/s,当前进度
                            var endSecond = DateTime.Now.Second;

                            if (beginSecond != endSecond)//计算速度
                            {
                                downSpeed = downSpeed / (endSecond - beginSecond);
                                //"下载速度" + downloadSpeed / 1024 + "KB/S";
                            }

                            action?.Invoke((downSpeed / 1024), downSize >= repLength ? 100 : Math.Max((int)(downSize * 100 / repLength), 1));

                            if (beginSecond != endSecond)
                            {
                                beginSecond = DateTime.Now.Second;      //重置
                                downSpeed = 0;                      //清空
                            }
                        };
                    }
                }
            });
        }

        #endregion

        /// <summary>
        /// 获取路径
        /// 路径 以 '/' 分割 
        /// </summary>
        public static string GetPath(string fileFullPath, bool isAppWork = false)
        {
            return GetPath("", fileFullPath, isAppWork: isAppWork);
        }

        /// <summary>
        /// 获取路径
        /// 路径 以 '/' 分割 
        /// </summary>
        public static string GetPath(string path, string fileName, bool isAppWork = false)
        {

            if (!path.IsEmpty())
            {
                //防止：fileName直接传入:E:/XXX类 
                path = path + "/";
            }
            if (isAppWork)
            {
                return Path.GetFullPath(string.Format("{0}/{1}{2}", AppContext.BaseDirectory, path, fileName));
            }
            return Path.GetFullPath(string.Format("{0}{1}", path, fileName));
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path"></param>
        public static void CreateFold(string path, bool isFilePath = false)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(isFilePath ? Path.GetDirectoryName(path) : path);
            }
        }

        /// <summary>
        /// 复制目录
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public static void CopyFold(string srcPath, string destPath, bool overwrite = true)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    string dests = GetPath(destPath, i.Name);
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(dests))
                        {
                            Directory.CreateDirectory(dests);                    //目标目录下不存在此文件夹即创建子文件夹
                        }
                        CopyFold(i.FullName, dests);                             //递归调用复制子文件夹
                    }
                    else
                    {
                        File.Copy(i.FullName, dests, overwrite);                 //不是文件夹即复制文件，true表示可以覆盖同名文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFold(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        public static void CopyFile(string sourceFileName, string destFileName, bool overwrite = true)
        {
            CreateFold(destFileName, isFilePath: true);
            File.Copy(sourceFileName, destFileName, overwrite);
        }



        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// 解压文件到指定目录
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="destFoldPath"></param>
        public static void ExtractFile(string filePath, string destFoldPath)
        {
            ZipFile.ExtractToDirectory(filePath, destFoldPath);
        }


        /// <summary>
        /// 读取一个文件(大小限制)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] ReadFile(string path)
        {
            byte[] datas = null;
            ReadFileCall(path, 0, 0, 0, (index, buffer, start, count, length) =>
            {
                if (index == 1)
                {
                    datas = buffer;
                }
                return false;
            });
            return datas;
        }

        /// <summary>
        /// 读取一个文件(大小限制)
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="begin">开始位置 0:从头开始</param>
        /// <param name="end">结束位置   0:读取到结束</param>
        /// <param name="chunk">分块读取 0:文件大小 超出int32 exception</param>
        /// <param name="size">文件大小</param>
        /// <returns></returns>
        public static byte[] ReadFile(string path, long begin, long end, int chunk, out long size)
        {
            byte[] datas = null;
            long fileSize = 0;
            ReadFileCall(path, begin, end, chunk, (index, buffer, start, count, length) =>
            {
                if (index == 1)
                {
                    datas = buffer;
                    fileSize = length;
                }
                return false;
            });
            size = fileSize;
            return datas;
        }

        /// <summary>
        /// 读取文件+读取回调
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="begin">开始位置 0:从头开始</param>
        /// <param name="end">结束位置   0:读取到结束</param>
        /// <param name="chunk">分块读取 0:文件大小 超出int32 exception</param>
        /// <param name="func">读取回调(序号：第x次读取, 字节：当前读取数据, 当前读取开始位置,当前读取字节大小, 文件总大小) 返回 true 继续 false 结束读取</param>
        public static void ReadFileCall(string path, long begin = 0, long end = 0, int chunk = 0, Func<int, byte[], long, int, long, bool> func = null)
        {
            path.CheckEmpty();

            if (chunk > Int32.MaxValue)
            {
                throw new FileLoadException(nameof(chunk));
            }
            FileStream fs = null;
            try
            {
                using (fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    begin = begin > end ? end : begin;
                    end = end == 0 && chunk > 0 ? begin + chunk : end;
                    end = end == 0 || end > fs.Length ? fs.Length : end;

                    long total = end - begin;
                    if (total > int.MaxValue)
                    {
                        throw new FileLoadException(nameof(total));
                    }

                    if (chunk == 0)
                    {
                        chunk = IntHelper.Get(total);
                    }
                    if (chunk > end)
                    {
                        chunk = IntHelper.Get(end);
                    }

                    byte[] buffer = new byte[chunk]; //创建接收文件内容的字节数组
                    long surplus = total;            //剩余未读
                    int num = 0;                     //每次实际返回的字节数长度
                    int index = 1;                   //chunk索引 当前读取第几个chunk
                    bool isGoOn = true;              //按chunk读，通过回调设置是否继续读下一chunk

                    while (isGoOn && surplus > 0)
                    {
                        fs.Seek(begin, SeekOrigin.Begin);
                        if (surplus < chunk)
                        {
                            buffer = new byte[surplus];
                            num = fs.Read(buffer, 0, Convert.ToInt32(surplus));
                        }
                        else
                        {
                            num = fs.Read(buffer, 0, chunk);
                        }

                        if (num == 0)
                        {
                            break;
                        }

                        if (func != null)
                        {
                            //第x次读取, 本次读取数据, 当前开始,当前读取字节大小,文件大小
                            isGoOn = func.Invoke(index, buffer, begin, num, total);
                        }

                        begin += num;
                        surplus -= num;
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs?.Close();
                fs?.Dispose();
            }
        }

        /// <summary>
        /// 导入Excel文件
        /// </summary>
        public static void ImportExcel(string filePath, Func<int, int, int, IRow, bool> func)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;
            FileStream fs = null;
            using (fs = File.OpenRead(filePath))
            {
                IWorkbook workbook = null;

                if (filePath.IndexOf(".xlsx") > 0)
                    workbook = new XSSFWorkbook(fs); //2007
                else if (filePath.IndexOf(".xls") > 0)
                    workbook = new HSSFWorkbook(fs); //2003

                if (workbook == null)
                    return;

                for (var i = 0; i < workbook.NumberOfSheets; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);
                    if (sheet != null)
                    {
                        var rowTotal = sheet.PhysicalNumberOfRows; // sheet.LastRowNum;

                        for (int j = 0; j < rowTotal; j++)
                        {
                            //假始返回tag 为 false，则跳出操作
                            var tag = func(i, j, rowTotal, sheet.GetRow(j));
                            if (!tag)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 导入Excel文件
        /// </summary>
        public static void ImportExcel(string dir, string name, Func<int, int, int, string, int, int, int, IRow, bool> func)
        {
            dir.CheckNull();
            name.CheckNull();
            string filePath = $"{dir}/{name}";

            if (string.IsNullOrWhiteSpace(filePath))
                return;
            FileStream fs = null;
            using (fs = File.OpenRead(filePath))
            {
                IWorkbook workbook = null;

                if (filePath.IndexOf(".xlsx") > 0)
                    workbook = new XSSFWorkbook(fs); //2007
                else if (filePath.IndexOf(".xls") > 0)
                    workbook = new HSSFWorkbook(fs); //2003

                if (workbook == null)
                    return;

                var sheets = workbook.NumberOfSheets;

                //Func<sheetTotal, curSheetName,curSheetIndex, rowTotal, rowIndex, IRow, bool>
                //func<int,string,int,int,int, IRow, bool>
                int total = 0;
                for (var i = 0; i < workbook.NumberOfSheets; i++)
                {
                    total = total + (workbook.GetSheetAt(i)?.PhysicalNumberOfRows).Value;
                }

                int current = 0;
                for (var i = 0; i < workbook.NumberOfSheets; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);
                    if (sheet != null)
                    {
                        var rowTotal = sheet.PhysicalNumberOfRows; // sheet.LastRowNum;
                        for (int j = 0; j < rowTotal; j++)
                        {
                            current++;

                            //假始返回tag 为 false，则跳出操作
                            var tag = func(total, current, workbook.NumberOfSheets, sheet.SheetName, i, rowTotal, j, sheet.GetRow(j));
                            if (!tag)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        #region 其它方式下载参考

        //namespace 文件下载_客户端_
        //    {
        //        public partial class Form1 : Form
        //        {
        //            public Form1()
        //            {
        //                InitializeComponent();
        //            }

        //            /// <summary>
        //            /// 直接下载
        //            /// </summary>
        //            /// <param name="sender"></param>
        //            /// <param name="e"></param>
        //            private async void button1_ClickAsync(object sender, EventArgs e)
        //            {
        //                using (HttpClient http = new HttpClient())
        //                {
        //                    var httpResponseMessage = await http.GetAsync("http://localhost:813/新建文件夹2.rar", HttpCompletionOption.ResponseHeadersRead);//发送请求
        //                    var contentLength = httpResponseMessage.Content.Headers.ContentLength;//读取文件大小
        //                    using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync())//读取文件流
        //                    {
        //                        var readLength = 1024000;//1000K  每次读取大小
        //                        byte[] bytes = new byte[readLength];
        //                        int writeLength;
        //                        while ((writeLength = stream.Read(bytes, 0, readLength)) > 0)//分块读取文件流
        //                        {
        //                            using (FileStream fs = new FileStream(Application.StartupPath + "/temp.rar", FileMode.Append, FileAccess.Write))//使用追加方式打开一个文件流
        //                            {
        //                                fs.Write(bytes, 0, writeLength);//追加写入文件
        //                                contentLength -= writeLength;
        //                                if (contentLength == 0)//如果写入完成 给出提示
        //                                    MessageBox.Show("下载完成");
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            /// <summary>
        //            /// 异步下载
        //            /// </summary>
        //            /// <param name="sender"></param>
        //            /// <param name="e"></param>
        //            private async void button2_ClickAsync(object sender, EventArgs e)
        //            {
        //                //开启一个异步线程
        //                await Task.Run(async () =>
        //                {
        //                    //异步操作UI元素
        //                    label1.Invoke((Action)(() =>
        //                    {
        //                        label1.Text = "准备下载...";
        //                    }));

        //                    long downloadSize = 0;//已经下载大小
        //                    long downloadSpeed = 0;//下载速度
        //                    using (HttpClient http = new HttpClient())
        //                    {
        //                        var httpResponseMessage = await http.GetAsync("http://localhost:813/新建文件夹2.rar", HttpCompletionOption.ResponseHeadersRead);//发送请求
        //                        var contentLength = httpResponseMessage.Content.Headers.ContentLength;   //文件大小                
        //                        using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync())
        //                        {
        //                            var readLength = 1024000;//1000K
        //                            byte[] bytes = new byte[readLength];
        //                            int writeLength;
        //                            var beginSecond = DateTime.Now.Second;//当前时间秒
        //                            while ((writeLength = stream.Read(bytes, 0, readLength)) > 0)
        //                            {
        //                                //使用追加方式打开一个文件流
        //                                using (FileStream fs = new FileStream(Application.StartupPath + "/temp.rar", FileMode.Append, FileAccess.Write))
        //                                {
        //                                    fs.Write(bytes, 0, writeLength);
        //                                }
        //                                downloadSize += writeLength;
        //                                downloadSpeed += writeLength;
        //                                progressBar1.Invoke((Action)(() =>
        //                                {
        //                                    var endSecond = DateTime.Now.Second;
        //                                    if (beginSecond != endSecond)//计算速度
        //                                    {
        //                                        downloadSpeed = downloadSpeed / (endSecond - beginSecond);
        //                                        label1.Text = "下载速度" + downloadSpeed / 1024 + "KB/S";

        //                                        beginSecond = DateTime.Now.Second;
        //                                        downloadSpeed = 0;//清空
        //                                    }
        //                                    progressBar1.Value = Math.Max((int)(downloadSize * 100 / contentLength), 1);
        //                                }));
        //                            }

        //                            label1.Invoke((Action)(() =>
        //                            {
        //                                label1.Text = "下载完成";
        //                            }));
        //                        }
        //                    }
        //                });
        //            }


        //            /// <summary>
        //            /// 是否暂停
        //            /// </summary>
        //            static bool isPause = true;
        //            /// <summary>
        //            /// 下载开始位置（也就是已经下载了的位置）
        //            /// </summary>
        //            static long rangeBegin = 0;//(当然，这个值也可以存为持久化。如文本、数据库等)

        //            /// <summary>
        //            /// 断线续传
        //            /// </summary>
        //            /// <param name="sender"></param>
        //            /// <param name="e"></param>
        //            private async void button3_ClickAsync(object sender, EventArgs e)
        //            {
        //                isPause = !isPause;
        //                if (!isPause)//点击下载
        //                {
        //                    button3.Text = "暂停";

        //                    await Task.Run(async () =>
        //                    {
        //                        //异步操作UI元素
        //                        label1.Invoke((Action)(() =>
        //                        {
        //                            label1.Text = "准备下载...";
        //                        }));

        //                        long downloadSpeed = 0;//下载速度
        //                        using (HttpClient http = new HttpClient())
        //                        {
        //                            //var url = "http://localhost:813/新建文件夹2.rar";  //a标签下载链接
        //                            var url = "http://localhost:813/FileDownload/FileDownload5";    //我们自己实现的服务端下载链接
        //                            var request = new HttpRequestMessage { RequestUri = new Uri(url) };
        //                            request.Headers.Range = new RangeHeaderValue(rangeBegin, null);//【关键点】全局变量记录已经下载了多少，然后下次从这个位置开始下载。
        //                            var httpResponseMessage = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        //                            var contentLength = httpResponseMessage.Content.Headers.ContentLength;//本次请求的内容大小
        //                            if (httpResponseMessage.Content.Headers.ContentRange != null) //如果为空，则说明服务器不支持断点续传
        //                            {
        //                                contentLength = httpResponseMessage.Content.Headers.ContentRange.Length;//服务器上的文件大小
        //                            }

        //                            using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync())
        //                            {
        //                                var readLength = 1024000;//1000K
        //                                byte[] bytes = new byte[readLength];
        //                                int writeLength;
        //                                var beginSecond = DateTime.Now.Second;//当前时间秒
        //                                while ((writeLength = stream.Read(bytes, 0, readLength)) > 0 && !isPause)
        //                                {
        //                                    //使用追加方式打开一个文件流
        //                                    using (FileStream fs = new FileStream(Application.StartupPath + "/temp.rar", FileMode.Append, FileAccess.Write))
        //                                    {
        //                                        fs.Write(bytes, 0, writeLength);
        //                                    }
        //                                    downloadSpeed += writeLength;
        //                                    rangeBegin += writeLength;
        //                                    progressBar1.Invoke((Action)(() =>
        //                                    {
        //                                        var endSecond = DateTime.Now.Second;
        //                                        if (beginSecond != endSecond)//计算速度
        //                                        {
        //                                            downloadSpeed = downloadSpeed / (endSecond - beginSecond);
        //                                            label1.Text = "下载速度" + downloadSpeed / 1024 + "KB/S";

        //                                            beginSecond = DateTime.Now.Second;
        //                                            downloadSpeed = 0;//清空
        //                                        }
        //                                        progressBar1.Value = Math.Max((int)((rangeBegin) * 100 / contentLength), 1);
        //                                    }));
        //                                }

        //                                if (rangeBegin == contentLength)
        //                                {
        //                                    label1.Invoke((Action)(() =>
        //                                    {
        //                                        label1.Text = "下载完成";
        //                                    }));
        //                                }
        //                            }
        //                        }
        //                    });
        //                }
        //                else//点击暂停
        //                {
        //                    button3.Text = "继续下载";
        //                    label1.Text = "暂停下载";
        //                }
        //            }

        //            /// <summary>
        //            /// 多线程下载文件
        //            /// </summary>
        //            /// <param name="sender"></param>
        //            /// <param name="e"></param>
        //            private async void button4_ClickAsync(object sender, EventArgs e)
        //            {
        //                using (HttpClient http = new HttpClient())
        //                {
        //                    var url = "http://localhost:813/FileDownload/FileDownload5";
        //                    var httpResponseMessage = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        //                    var contentLength = httpResponseMessage.Content.Headers.ContentLength.Value;
        //                    var size = contentLength / 10; //这里为了方便，就直接分成10个线程下载。（当然这是不合理的）
        //                    var tasks = new List<Task>();
        //                    for (int i = 0; i < 10; i++)
        //                    {
        //                        var begin = i * size;
        //                        var end = begin + size - 1;
        //                        var task = FileDownload(url, begin, end, i);
        //                        tasks.Add(task);
        //                    }
        //                    for (int i = 0; i < 10; i++)
        //                    {
        //                        await tasks[i];  //当然，这里如有下载异常没有考虑、文件也没有校验。各位自己完善吧。
        //                        progressBar1.Value = (i + 1) * 10;
        //                    }
        //                    FileMerge(Application.StartupPath + @"\File", "temp.rar");
        //                    label1.Text = "下载完成";
        //                }
        //            }

        //            /// <summary>
        //            /// 文件下载
        //            /// （如果你有兴趣，可以没个线程弄个进度条）
        //            /// </summary>
        //            /// <param name="begin"></param>
        //            /// <param name="end"></param>
        //            /// <param name="index"></param>
        //            /// <returns></returns>
        //            public Task FileDownload(string url, long begin, long end, int index)
        //            {
        //                var task = Task.Run(async () =>
        //                {
        //                    using (HttpClient http = new HttpClient())
        //                    {
        //                        var request = new HttpRequestMessage { RequestUri = new Uri(url) };
        //                        request.Headers.Range = new RangeHeaderValue(begin, end);//【关键点】全局变量记录已经下载了多少，然后下次从这个位置开始下载。
        //                        var httpResponseMessage = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        //                        using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync())
        //                        {
        //                            var readLength = 1024000;//1000K
        //                            byte[] bytes = new byte[readLength];
        //                            int writeLength;
        //                            var beginSecond = DateTime.Now.Second;//当前时间秒
        //                            var filePaht = Application.StartupPath + "/File/";
        //                            if (!Directory.Exists(filePaht))
        //                                Directory.CreateDirectory(filePaht);

        //                            try
        //                            {
        //                                while ((writeLength = stream.Read(bytes, 0, readLength)) > 0)
        //                                {
        //                                    //使用追加方式打开一个文件流
        //                                    using (FileStream fs = new FileStream(filePaht + index, FileMode.Append, FileAccess.Write))
        //                                    {
        //                                        fs.Write(bytes, 0, writeLength);
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception)
        //                            {
        //                                //如果出现异常则删掉这个文件
        //                                File.Delete(filePaht + index);
        //                            }
        //                        }
        //                    }
        //                });

        //                return task;
        //            }

        //            /// <summary>
        //            /// 合并文件
        //            /// </summary>
        //            /// <param name="path"></param>
        //            /// <returns></returns>
        //            public bool FileMerge(string path, string fileName)
        //            {
        //                //这里排序一定要正确，转成数字后排序（字符串会按1 10 11排序，默认10比2小）
        //                foreach (var filePath in Directory.GetFiles(path).OrderBy(t => int.Parse(Path.GetFileNameWithoutExtension(t))))
        //                {
        //                    using (FileStream fs = new FileStream(Directory.GetParent(path).FullName + @"\" + fileName, FileMode.Append, FileAccess.Write))
        //                    {
        //                        byte[] bytes = System.IO.File.ReadAllBytes(filePath);//读取文件到字节数组
        //                        fs.Write(bytes, 0, bytes.Length);//写入文件
        //                    }
        //                    System.IO.File.Delete(filePath);
        //                }
        //                Directory.Delete(path);
        //                return true;
        //            }
        //        }
        //    }

        #endregion
    }
}
