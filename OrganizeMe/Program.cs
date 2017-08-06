using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace OrganizeMe
{
    class Program
    {
        public static NotifyIcon notifyIcon;
        public static ContextMenu trayMenu;
        public static bool areWeStoppingThis;

        static void Main(string[] args)
        {
            Config conf = Config.LoadSettings();

            if (conf.IsSurveillanceMode)
            {
                StartSurveillanceMode(conf);
            }

            MoveFiles(conf.Folders,conf.DirectoriesUnderSurveillance);
        }

        private static void StartSurveillanceMode(Config conf)
        {
            DateTime dtLastModif = DateTime.Now;

            HandleNotifyIcon();

            // Todo Changer le while true , not good ! 
            while (!areWeStoppingThis)
            {
                foreach (string targetFolder in conf.DirectoriesUnderSurveillance)
                {
                    DirectoryInfo dr = new DirectoryInfo(targetFolder);

                    if (dr.LastWriteTime > dtLastModif)
                    {
                        StartTransfert(conf.Folders,targetFolder);
                        dtLastModif = dr.LastWriteTime;
                    }

                    Thread.Sleep((int)conf.TimeLapse);
                }              
            };
        }

        private static void HandleNotifyIcon()
        {
            Thread notifyThread = new Thread(
                () =>
                {
                    trayMenu = new ContextMenu();
                    trayMenu.MenuItems.Add("Stop", notifyIcon_Stop);

                    notifyIcon = new NotifyIcon()
                    { 
                        Text = "Test",
                        Icon = new Icon("fileIco.ico"),
                        ContextMenu = trayMenu,
                        Visible = true
                    };
 
                    Application.Run();
                });

            notifyThread.Start();
        }

        private static void notifyIcon_Stop(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            MessageBox.Show("programme terminé");
            areWeStoppingThis = true;
            Application.Exit();
        }

        private static void MoveFiles(List<Folder> folders , List<string> directoriesUnderSurveillance)
        {

            int movedFilesCount = 0;
            foreach (string underSurveillanceFolder in directoriesUnderSurveillance)
            {
                movedFilesCount += StartTransfert(folders, underSurveillanceFolder); 
            }

            //Recap
            Console.WriteLine(movedFilesCount + " fichiers transférés ");
        }

        private static int StartTransfert(List<Folder> folders, string underSurveillanceFolder)
        {
            int movedFilesCount = 0;

            if (!Directory.Exists(underSurveillanceFolder))
            {
                Console.WriteLine("{0} introuvable ... suivant", underSurveillanceFolder);
                return movedFilesCount;
            }

            DirectoryInfo dirInfos = new DirectoryInfo(underSurveillanceFolder);

            //récupération des fichiers présent dans le repertoire
            string[] files = Directory.GetFiles(underSurveillanceFolder, "*.*");

            if (files.Length == 0)
                Console.WriteLine("{0} => Aucun fichier n'est présent ... suivant ", dirInfos.Name);

            Console.WriteLine(" -- Nombre de fichiers => {0}\n", files.Length);

            Console.WriteLine(" -- Transfert des fichiers DEBUT\n");
            foreach (string file in files)
            {
                bool sucess = false;
                FileInfo fileInfo = new FileInfo(file);

                // on récupère le folder ciblé selon l'extension
                Folder folder = folders.FirstOrDefault(f => f.Extensions.Contains(fileInfo.Extension));

                if (folder != null)
                    TransfertFile(folder, fileInfo, out sucess);

                if (sucess) movedFilesCount++;

            }
            Console.WriteLine(" -- Transfert des fichiers FIN");

            return movedFilesCount;
        }

        private static void TransfertFile(Folder folder, FileInfo fileInfo,out bool success)
        {
            success = false;
            string path = Path.Combine(folder.Path, fileInfo.Name);
            Console.ForegroundColor = folder.Color;
            Console.WriteLine("\t  {0} ---> {1}", fileInfo.Name, folder.Name);
            Console.ResetColor();

            //si le dossier n'existe pas on le créé
            if (!Directory.Exists(folder.Path))
                Directory.CreateDirectory(folder.Path);

            try
            {
                File.Move(fileInfo.FullName, path);
                success = true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t [[ Une erreur est survenue lors de l'envoie : {0} ]]", ex.Message);
                Console.ResetColor();

            }
        }
    }
}
