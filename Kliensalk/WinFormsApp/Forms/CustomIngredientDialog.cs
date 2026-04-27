using System;
using System.Windows.Forms;

namespace NaturaCo.RecipeEditor.Forms
{
    public partial class CustomIngredientDialog : Form
    {
        public string  IngredientName => txtName.Text.Trim();
        public decimal Amount         => (decimal)nudAmount.Value;
        public string  Unit           => cmbUnit.Text.Trim();

        public CustomIngredientDialog()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("A hozzávaló nevét meg kell adni.", "Hiányzó adat",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btnOk_Click(sender, e);
        }
    }
}
