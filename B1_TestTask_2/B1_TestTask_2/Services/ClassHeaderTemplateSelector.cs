using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using B1_TestTask_2.ViewModels;

namespace B1_TestTask_2.Services
{
    public class ClassHeaderTemplateSelector : DataTemplateSelector
    {
        // Шаблон для заголовка класса
        public DataTemplate ClassHeaderTemplate { get; set; }

        // Обычный шаблон для отображения данных
        public DataTemplate NormalTemplate { get; set; }

        // Переопределение метода выбора шаблона для элемента
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Преобразование объекта в модель отображения счета
            var record = item as AccountDisplayModel;

            // Проверка, является ли элемент заголовком класса
            if (record != null && record.IsClassHeader)
            {
                // Возврат шаблона для заголовка класса
                return ClassHeaderTemplate;
            }
            else
            {
                // Возврат обычного шаблона для отображения данных
                return NormalTemplate;
            }
        }

    }
}
