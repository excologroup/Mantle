﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Mantle.Configuration.Attributes;
using Mantle.Configuration.Interfaces;
using Mantle.Extensions;
using Mantle.Interfaces;

namespace Mantle.Configuration.Configurers
{
    public abstract class BaseConfigurer<T> : IConfigurer<T>
    {
        protected readonly TypeMetadata TypeMetadata;

        protected BaseConfigurer()
        {
            TypeMetadata = new TypeMetadata(typeof (T));
        }

        protected BaseConfigurer(ITypeMetadataCache typeMetadataCache)
        {
            TypeMetadata = typeMetadataCache.GetTypeMetadata(typeof (T));
        }

        public virtual T Configure(T target, string targetName = null)
        {
            ConfigurationTarget<T> cfgTarget = ToConfigurationTarget(target, targetName);
            IEnumerable<ConfigurationSetting> cfgSettings = GetConfigurationSettings(cfgTarget);

            ApplyConfigurationSettings(cfgTarget, cfgSettings);

            return cfgTarget.Target;
        }

        public abstract IEnumerable<ConfigurationSetting> GetConfigurationSettings(ConfigurationTarget<T> cfgTarget);

        protected virtual ConfigurationTarget<T> ToConfigurationTarget(T target, string targetName = null)
        {
            var cfgTarget = new ConfigurationTarget<T>();

            cfgTarget.Name = targetName;
            cfgTarget.Target = target;
            cfgTarget.TypeMetadata = TypeMetadata;

            foreach (PropertyMetadata propertyMetadata in TypeMetadata.Properties)
            {
                ConfigurableAttribute cfgAttribute =
                    propertyMetadata.Attributes.OfType<ConfigurableAttribute>().SingleOrDefault();

                if (cfgAttribute != null)
                {
                    var cfgTargetProperty = new ConfigurationTargetProperty();

                    cfgTargetProperty.IsRequired = cfgAttribute.IsRequired;
                    cfgTargetProperty.PropertyMetadata = propertyMetadata;

                    cfgTargetProperty.SettingName = (String.IsNullOrEmpty(cfgAttribute.SettingName)
                        ? GetConventionalSettingName(cfgTarget, propertyMetadata)
                        : cfgAttribute.SettingName.Merge(new {Name = targetName}));
                }
            }

            return cfgTarget;
        }

        protected virtual ConfigurationTarget<T> ApplyConfigurationSettings(ConfigurationTarget<T> cfgTarget,
                                                                            IEnumerable<ConfigurationSetting>
                                                                                cfgSettings)
        {
            List<ConfigurationSetting> cfgSettingsList = cfgSettings.ToList();

            foreach (ConfigurationTargetProperty cfgTargetProperty in cfgTarget.Properties)
            {
                ConfigurationSetting cfgSetting =
                    cfgSettingsList.SingleOrDefault(s => (s.Name == cfgTargetProperty.SettingName));

                if (cfgSetting != null)
                {
                    ApplyConfigurationSetting(cfgTarget, cfgTargetProperty, cfgSetting);
                }
                else if (cfgTargetProperty.IsRequired)
                {
                    throw new ConfigurationErrorsException(String.Format("[{0}] is not configured.",
                        cfgTargetProperty.SettingName));
                }
            }

            return cfgTarget;
        }

        protected virtual string GetConventionalSettingName(ConfigurationTarget<T> cfgTarget,
                                                            PropertyMetadata propertyMetadata)
        {
            if (String.IsNullOrEmpty(cfgTarget.Name))
            {
                return String.Format("{0}.{1}", cfgTarget.TypeMetadata.Type.Name,
                    propertyMetadata.PropertyInfo.Name);
            }

            return String.Format("{0}.{1}.{2}", cfgTarget.Name, cfgTarget.TypeMetadata.Type.Name,
                propertyMetadata.PropertyInfo.Name);
        }

        private void ApplyConfigurationSetting(ConfigurationTarget<T> cfgTarget,
                                               ConfigurationTargetProperty cfgTargetProperty,
                                               ConfigurationSetting cfgSetting)
        {
            Type propertyType = cfgTargetProperty.PropertyMetadata.PropertyInfo.PropertyType;

            if ((propertyType == typeof (bool)) || (propertyType == typeof (bool?)))
            {
                ApplyBooleanConfigurationSetting(cfgTarget, cfgTargetProperty, cfgSetting);
            }
            else if ((propertyType == typeof (DateTime)) || (propertyType == typeof (DateTime?)))
            {
                ApplyDateTimeConfigurationSetting(cfgTarget, cfgTargetProperty, cfgSetting);
            }
            else if ((propertyType == typeof (double)) || (propertyType == typeof (double?)))
            {
                ApplyDoubleConfigurationSetting(cfgTarget, cfgTargetProperty, cfgSetting);
            }
            else if ((propertyType == typeof (Guid)) || (propertyType == typeof (Guid?)))
            {
                ApplyGuidConfigurationSetting(cfgTarget, cfgTargetProperty, cfgSetting);
            }
            else if ((propertyType == typeof (int)) || (propertyType == typeof (int?)))
            {
                ApplyIntConfigurationSetting(cfgTarget, cfgTargetProperty, cfgSetting);
            }
            else if ((propertyType == typeof (long)) || (propertyType == typeof (long?)))
            {
                ApplyLongConfigurationSettings(cfgTarget, cfgTargetProperty, cfgSetting);
            }
            else if ((propertyType == typeof (string)))
            {
                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, cfgSetting.Value);
            }
            else
            {
                throw new ConfigurationErrorsException(
                    String.Format(
                        "Unable to apply configuration setting [{0}: {1}] to property [{2}/{3}]. The target property must be of type 'bool', 'bool?', 'DateTime', 'DateTime?', 'double', 'double?', 'Guid', 'Guid?', 'int', 'int?', 'long', 'long?' or 'string'.",
                        cfgSetting.Name, cfgSetting.Value, cfgTarget.TypeMetadata.Type.Name,
                        cfgTargetProperty.PropertyMetadata.PropertyInfo.Name));
            }
        }

        private void ApplyBooleanConfigurationSetting(ConfigurationTarget<T> cfgTarget,
                                                      ConfigurationTargetProperty cfgTargetProperty,
                                                      ConfigurationSetting cfgSetting)
        {
            Type propertyType = cfgTargetProperty.PropertyMetadata.PropertyInfo.PropertyType;
            bool? value = cfgSetting.Value.TryParseBoolean();

            if (propertyType == (typeof (bool)))
            {
                if (value == null)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "Unable to apply configuration setting [{0}: {1}] to property [{2}/{3}]. [{1}] can not be converted to a boolean value.",
                        cfgSetting.Name, cfgSetting.Value, cfgTarget.TypeMetadata.Type.Name,
                        cfgTargetProperty.PropertyMetadata.PropertyInfo.Name));
                }

                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value.Value);
            }
            else if (propertyType == (typeof (bool?)))
            {
                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value);
            }
        }

        private void ApplyDateTimeConfigurationSetting(ConfigurationTarget<T> cfgTarget,
                                                       ConfigurationTargetProperty cfgTargetProperty,
                                                       ConfigurationSetting cfgSetting)
        {
            Type propertyType = cfgTargetProperty.PropertyMetadata.PropertyInfo.PropertyType;
            DateTime? value = cfgSetting.Value.TryParseDateTime();

            if (propertyType == (typeof (DateTime)))
            {
                if (value == null)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "Unable to apply configuration setting [{0}: {1}] to property [{2}/{3}]. [{1}] can not be converted to a date/time value.",
                        cfgSetting.Name, cfgSetting.Value, cfgTarget.TypeMetadata.Type.Name,
                        cfgTargetProperty.PropertyMetadata.PropertyInfo.Name));
                }

                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value.Value);
            }
            else if (propertyType == (typeof (DateTime?)))
            {
                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value);
            }
        }

        private void ApplyDoubleConfigurationSetting(ConfigurationTarget<T> cfgTarget,
                                                     ConfigurationTargetProperty cfgTargetProperty,
                                                     ConfigurationSetting cfgSetting)
        {
            Type propertyType = cfgTargetProperty.PropertyMetadata.PropertyInfo.PropertyType;
            double? value = cfgSetting.Value.TryParseDouble();

            if (propertyType == (typeof (double)))
            {
                if (value == null)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "Unable to apply configuration setting [{0}: {1}] to property [{2}/{3}]. [{1}] can not be converted to a double value.",
                        cfgSetting.Name, cfgSetting.Value, cfgTarget.TypeMetadata.Type.Name,
                        cfgTargetProperty.PropertyMetadata.PropertyInfo.Name));
                }

                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value.Value);
            }
            else if (propertyType == (typeof (double?)))
            {
                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget, value);
            }
        }

        private void ApplyGuidConfigurationSetting(ConfigurationTarget<T> cfgTarget,
                                                   ConfigurationTargetProperty cfgTargetProperty,
                                                   ConfigurationSetting cfgSetting)
        {
            Type propertyType = cfgTargetProperty.PropertyMetadata.PropertyInfo.PropertyType;
            Guid? value = cfgSetting.Value.TryParseGuid();

            if (propertyType == (typeof (Guid)))
            {
                if (value == null)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "Unable to apply configuration setting [{0}: {1}] to property [{2}/{3}]. [{1}] can not be converted to a GUID value.",
                        cfgSetting.Name, cfgSetting.Value, cfgTarget.TypeMetadata.Type.Name,
                        cfgTargetProperty.PropertyMetadata.PropertyInfo.Name));
                }

                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value.Value);
            }
            else if (propertyType == (typeof (Guid?)))
            {
                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value);
            }
        }

        private void ApplyIntConfigurationSetting(ConfigurationTarget<T> cfgTarget,
                                                  ConfigurationTargetProperty cfgTargetProperty,
                                                  ConfigurationSetting cfgSetting)
        {
            Type propertyType = cfgTargetProperty.PropertyMetadata.PropertyInfo.PropertyType;
            int? value = cfgSetting.Value.TryParseInt();

            if (propertyType == (typeof (int)))
            {
                if (value == null)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "Unable to apply configuration setting [{0}: {1}] to property [{2}/{3}]. [{1}] can not be converted to a 32-bit integer value.",
                        cfgSetting.Name, cfgSetting.Value, cfgTarget.TypeMetadata.Type.Name,
                        cfgTargetProperty.PropertyMetadata.PropertyInfo.Name));
                }

                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value.Value);
            }
            else if (propertyType == (typeof (int?)))
            {
                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value);
            }
        }

        private void ApplyLongConfigurationSettings(ConfigurationTarget<T> cfgTarget,
                                                    ConfigurationTargetProperty cfgTargetProperty,
                                                    ConfigurationSetting cfgSetting)
        {
            Type propertyType = cfgTargetProperty.PropertyMetadata.PropertyInfo.PropertyType;
            long? value = cfgSetting.Value.TryParseLong();

            if (propertyType == (typeof (long)))
            {
                if (value == null)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "Unable to apply configuration setting [{0}: {1}] to property [{2}/{3}]. [{1}] can not be converted to a 64-bit integer value.",
                        cfgSetting.Name, cfgSetting.Value, cfgTarget.TypeMetadata.Type.Name,
                        cfgTargetProperty.PropertyMetadata.PropertyInfo.Name));
                }

                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value.Value);
            }
            else if (propertyType == (typeof (long?)))
            {
                cfgTargetProperty.PropertyMetadata.PropertyInfo.SetValue(cfgTarget.Target, value);
            }
        }
    }
}