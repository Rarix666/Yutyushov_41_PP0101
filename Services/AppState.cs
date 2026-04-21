using AISDisciplineDesc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AISDisciplineDesc.Services
{
    internal class AppState
    {
        public static SupabaseClient Supabase { get; set; }
        public static UserData CurrentUser { get; set; }
        public static List<Documents> Documentation { get; set; }
        public static List<AdminData> AdminDataUsers { get; set; }
        public static List<Divisions> divisions { get; set; }
        public static List<Units> units { get; set; }
        public static async Task LoadDivisionsAsync() // Общий метод загрузки подразделений
        {
            try
            {
                if (divisions == null || divisions.Count == 0)
                {
                    bool success = await Supabase.DivInformation();
                    if (!success)
                    {
                        MessageBox.Show("Ошибка загрузки подразделений");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        public static async Task LoadUnitsAsync() // Общий метод загрузки частей
        {
            try
            {
                if (units == null || units.Count == 0)
                {
                    bool success = await Supabase.UnitInformation();
                    if (!success)
                    {
                        MessageBox.Show("Ошибка загрузки подразделений");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
