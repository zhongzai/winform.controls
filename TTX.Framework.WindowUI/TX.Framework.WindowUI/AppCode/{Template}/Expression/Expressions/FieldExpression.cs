#region COPYRIGHT
//
//     THIS IS GENERATED BY TEMPLATE
//     
//     AUTHOR  :     ROYE
//     DATE       :     2010
//
//     COPYRIGHT (C) 2010, TIANXIAHOTEL TECHNOLOGIES CO., LTD. ALL RIGHTS RESERVED.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;

namespace System.Text.Template
{
    public delegate bool MissingFieldHandler(object targetObject, Type targetType, string member, out object fieldValue, out Type fieldType);

    public class FieldExpression : Expression
    {
        private readonly Expression _target;
        private readonly string _member;
        private readonly MissingFieldHandler _missingFieldHandler;
        private readonly MissingFieldHandler _interceptMemberHandler;

        public FieldExpression(Expression target, string member)
        {
            _target = target;
            _member = member;
        }

        public FieldExpression(Expression target, string member, MissingFieldHandler missingMemberHandler, MissingFieldHandler interceptMemberHandler)
        {
            _target = target;
            _member = member;
            _missingFieldHandler = missingMemberHandler;
            _interceptMemberHandler = interceptMemberHandler;
        }

        public override ValueExpression Evaluate(ITemplateContext context)
        {
            return Evaluate(context, null);
        }

        public ValueExpression Assign(ITemplateContext context, object value)
        {
            return Evaluate(context, value);
        }

        private ValueExpression Evaluate(ITemplateContext context, object newValue)
        {
            ValueExpression targetValue = _target.Evaluate(context);
            object targetObject;
            Type targetType;

            if (targetValue.Value is ClassName)
            {
                targetType = ((ClassName)targetValue.Value).Type;
                targetObject = null;
            }
            else
            {
                targetType = targetValue.Type;
                targetObject = targetValue.Value;

                if (targetObject == null)
                    return new ValueExpression(null, targetType);
            }

            //if (targetObject is IDynamicObject)
            //{
            //    object value;
            //    Type type;

            //    if (((IDynamicObject)targetObject).TryGetValue(_member, out value, out type))
            //        return new ValueExpression(value, type);
            //}

            MemberInfo[] members = targetType.GetMember(_member);
            if (members.Length == 0)
            {
                PropertyInfo indexerPropInfo = targetType.GetProperty("Item", new Type[] { typeof(string) });

                if (indexerPropInfo != null)
                {
                    return new ValueExpression(indexerPropInfo.GetValue(targetObject, new object[] { _member }), indexerPropInfo.PropertyType);
                }

                if (_missingFieldHandler != null)
                {
                    foreach (MissingFieldHandler handler in _missingFieldHandler.GetInvocationList())
                    {
                        object value;
                        Type type;

                        if (handler(targetObject, targetType, _member, out value, out type))
                            return new ValueExpression(value, type);
                    }
                }

                throw new MissingFieldException(targetType.Name, _member);
            }

            if (members.Length >= 1 && members[0] is MethodInfo)
            {
                if (targetObject == null)
                    return Expression.Value(new StaticMethod(targetType, _member));
                else
                    return Expression.Value(new InstanceMethod(targetType, _member, targetObject));
            }

            MemberInfo member = members[0];
            if (members.Length > 1 && targetObject != null) // CoolStorage, ActiveRecord and Dynamic Proxy frameworks sometimes return > 1 member
            {
                foreach (MemberInfo mi in members)
                {
                    if (mi.DeclaringType == targetObject.GetType())
                        member = mi;
                }
            }

            if (newValue != null)
            {
                if (member is FieldInfo)
                    ((FieldInfo)member).SetValue(targetObject, newValue);
                if (member is PropertyInfo)
                    ((PropertyInfo)member).SetValue(targetObject, newValue, null);
                // Fall through to get the new property/field value below
            }

            if (member is FieldInfo)
            {
                return new ValueExpression(((FieldInfo)member).GetValue(targetObject), ((FieldInfo)member).FieldType);
            }
            if (member is PropertyInfo)
            {
                return new ValueExpression(((PropertyInfo)member).GetValue(targetObject, null), ((PropertyInfo)member).PropertyType);
            }
            throw new InvalidExpressionException();
        }

       

        public override string ToString()
        {
            return "(" + _target + "." + _member + ")";
        }
    }
}
