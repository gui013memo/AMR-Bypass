namespace AMR_Bypass
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // UI Controls
        private System.Windows.Forms.GroupBox groupBoxControls;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Label lblLeftStatus;
        private System.Windows.Forms.Label lblRightStatus;
        private System.Windows.Forms.TextBox txtStatusLog;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblLeft;
        private System.Windows.Forms.Label lblRight;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                // Dispose of any other resources here
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBoxControls = new GroupBox();
            lblRightStatus = new Label();
            lblLeftStatus = new Label();
            btnRight = new Button();
            btnLeft = new Button();
            lblRight = new Label();
            lblLeft = new Label();
            txtStatusLog = new TextBox();
            btnExit = new Button();
            groupBoxControls.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxControls
            // 
            groupBoxControls.Controls.Add(lblRightStatus);
            groupBoxControls.Controls.Add(lblLeftStatus);
            groupBoxControls.Controls.Add(btnRight);
            groupBoxControls.Controls.Add(btnLeft);
            groupBoxControls.Controls.Add(lblRight);
            groupBoxControls.Controls.Add(lblLeft);
            groupBoxControls.Location = new Point(12, 12);
            groupBoxControls.Name = "groupBoxControls";
            groupBoxControls.Size = new Size(533, 194);
            groupBoxControls.TabIndex = 0;
            groupBoxControls.TabStop = false;
            groupBoxControls.Text = "FORÇAR SENSORES";
            // 
            // lblRightStatus
            // 
            lblRightStatus.AutoSize = true;
            lblRightStatus.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
            lblRightStatus.ForeColor = Color.Red;
            lblRightStatus.Location = new Point(418, 20);
            lblRightStatus.Name = "lblRightStatus";
            lblRightStatus.Size = new Size(78, 45);
            lblRightStatus.TabIndex = 5;
            lblRightStatus.Text = "OFF";
            // 
            // lblLeftStatus
            // 
            lblLeftStatus.AutoSize = true;
            lblLeftStatus.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
            lblLeftStatus.ForeColor = Color.Red;
            lblLeftStatus.Location = new Point(110, 20);
            lblLeftStatus.Name = "lblLeftStatus";
            lblLeftStatus.Size = new Size(78, 45);
            lblLeftStatus.TabIndex = 4;
            lblLeftStatus.Text = "OFF";
            // 
            // btnRight
            // 
            btnRight.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point);
            btnRight.Location = new Point(306, 68);
            btnRight.Name = "btnRight";
            btnRight.Size = new Size(202, 120);
            btnRight.TabIndex = 3;
            btnRight.Text = "Toggle RIGHT";
            btnRight.UseVisualStyleBackColor = true;
            btnRight.Click += btnRight_Click;
            // 
            // btnLeft
            // 
            btnLeft.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point);
            btnLeft.Location = new Point(20, 68);
            btnLeft.Name = "btnLeft";
            btnLeft.Size = new Size(202, 120);
            btnLeft.TabIndex = 2;
            btnLeft.Text = "Toggle LEFT";
            btnLeft.UseVisualStyleBackColor = true;
            btnLeft.Click += btnLeft_Click;
            // 
            // lblRight
            // 
            lblRight.AutoSize = true;
            lblRight.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point);
            lblRight.Location = new Point(332, 29);
            lblRight.Name = "lblRight";
            lblRight.Size = new Size(80, 32);
            lblRight.TabIndex = 1;
            lblRight.Text = "RIGHT";
            // 
            // lblLeft
            // 
            lblLeft.AutoSize = true;
            lblLeft.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point);
            lblLeft.Location = new Point(42, 29);
            lblLeft.Name = "lblLeft";
            lblLeft.Size = new Size(62, 32);
            lblLeft.TabIndex = 0;
            lblLeft.Text = "LEFT";
            // 
            // txtStatusLog
            // 
            txtStatusLog.Location = new Point(13, 212);
            txtStatusLog.Multiline = true;
            txtStatusLog.Name = "txtStatusLog";
            txtStatusLog.ReadOnly = true;
            txtStatusLog.ScrollBars = ScrollBars.Vertical;
            txtStatusLog.Size = new Size(533, 285);
            txtStatusLog.TabIndex = 1;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.Firebrick;
            btnExit.Font = new Font("Segoe UI", 32F, FontStyle.Regular, GraphicsUnit.Point);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(12, 503);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(533, 91);
            btnExit.TabIndex = 2;
            btnExit.Text = "ENCERRAR BYPASS";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // Form1
            // 
            AcceptButton = btnLeft;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(558, 606);
            Controls.Add(btnExit);
            Controls.Add(txtStatusLog);
            Controls.Add(groupBoxControls);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "Form1";
            Text = "AMR Manual Bypass (AMB)";
            groupBoxControls.ResumeLayout(false);
            groupBoxControls.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
