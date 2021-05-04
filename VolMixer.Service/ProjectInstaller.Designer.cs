
namespace VolMixer.Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.VolMixerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.VolMixerInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // VolMixerServiceProcessInstaller
            // 
            this.VolMixerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.VolMixerServiceProcessInstaller.Password = null;
            this.VolMixerServiceProcessInstaller.Username = null;
            // 
            // VolMixerInstaller
            // 
            this.VolMixerInstaller.DisplayName = "VolMixer";
            this.VolMixerInstaller.ServiceName = "VolMixer";
            this.VolMixerInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.VolMixerInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.VolMixerServiceProcessInstaller,
            this.VolMixerInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller VolMixerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller VolMixerInstaller;
    }
}