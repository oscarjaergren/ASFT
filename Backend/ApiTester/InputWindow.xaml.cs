using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ApiTester
{
	public partial class InputWindow : Window
	{
		private readonly object _model;

		public InputWindow( object model )
		{
			InitializeComponent();

			_model = model;
			var type = _model.GetType();
			Title = type.Name;
			InitControls( type.GetProperties() );
		}

		private void InitControls( PropertyInfo[] properties )
		{
			foreach (var property in properties)
			{
				var label = new Label();
				label.Content = property.Name;
				InputControls.Children.Add( label );

				var propType = property.PropertyType;
				if ( propType == typeof( string ) )
				{
					var textBox = new TextBox();
					textBox.Text = (string)property.GetValue( _model );

					textBox.TextChanged += ( sender, e ) =>
											{
												property.SetValue( _model, textBox.Text );
											};
					InputControls.Children.Add( textBox );
				}
				else if ( propType == typeof( int ) )
				{
					var textBox = new TextBox();
					textBox.Text = property.GetValue( _model ).ToString();

					textBox.TextChanged += ( sender, e ) =>
					{
						int value;
						if (int.TryParse(textBox.Text, out value))
						{
							textBox.Foreground = Brushes.Black;
							property.SetValue( _model, value );
						}
						else
							textBox.Foreground = Brushes.Red;
					};
					InputControls.Children.Add( textBox );
				}
				else if ( propType == typeof( float ) || propType == typeof( double ) || propType == typeof( decimal ) )
				{
					var textBox = new TextBox();
					textBox.Text = property.GetValue( _model ).ToString();
					
					textBox.TextChanged += ( sender, e ) =>
					{
						decimal value;
						if ( decimal.TryParse( textBox.Text, out value ) )
						{
							textBox.Foreground = Brushes.Black;
							if ( propType == typeof( float ) )
								property.SetValue( _model, (float)value );
							if ( propType == typeof( double ) )
								property.SetValue( _model, (double)value );
							else
								property.SetValue( _model, value );
						}
						else
							textBox.Foreground = Brushes.Red;
					};

					InputControls.Children.Add( textBox );
				}
				else if ( propType == typeof( bool ) )
				{
					var checkBox = new CheckBox();
					checkBox.IsChecked = (bool)property.GetValue( _model );

					var eventHandler = new RoutedEventHandler(( sender, e ) =>
					{
						property.SetValue( _model, checkBox.IsChecked == true );
					});
					checkBox.Checked += eventHandler;
					checkBox.Unchecked += eventHandler;

					InputControls.Children.Add( checkBox );
				}
				else
				{
					if (propType.IsEnum)
					{
						var comboBox = new ComboBox();
						comboBox.Background = Brushes.LightBlue;

						var values = propType.GetEnumValues().OfType<object>().ToList();
						foreach (var value in values)
						{
							var item = new ComboBoxItem();
							item.Content = value;
							comboBox.Items.Add( item );
						}

						comboBox.SelectionChanged += (sender, args) =>
						{
							property.SetValue( _model, ((ComboBoxItem)comboBox.SelectedItem).Content );
						};

						comboBox.SelectedItem = comboBox.Items[0];

						InputControls.Children.Add( comboBox );
					}
				}
			}
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}
	}
}
