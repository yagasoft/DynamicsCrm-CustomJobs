// =====================================================================
//  This file is part of the Microsoft Dynamics CRM Technical Article code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
// =====================================================================

SDK = window.SDK || { __namespace: true };
SDK.Metadata = SDK.Metadata || { __namespace: true };
SDK.Metadata.Query = SDK.Metadata.Query || { __namespace: true };
SDK.Metadata.Query.ValueEnums = SDK.Metadata.Query.ValueEnums || { __namespace: true };

//Use this file to assemble the library from each of the debug libraries
(function () {
 // SDK.Metadata.Query.AttributeQueryExpression START
	 this.AttributeQueryExpression = function (criteria, properties) {
   ///<summary>
  /// Defines a complex query to retrieve attribute metadata for entities retrieved using an SDK.Metadata.Query.EntityQueryExpression
  ///</summary>
  ///<param name="criteria" type="SDK.Metadata.Query.MetadataFilterExpression">
  /// The filter criteria for the metadata query
  ///</param>
  ///<param name="properties" type="SDK.Metadata.Query.MetadataPropertiesExpression">
  /// The properties to be returned by the query
  ///</param>
  if (!(this instanceof SDK.Metadata.Query.AttributeQueryExpression)) {
   return new SDK.Metadata.Query.AttributeQueryExpression(criteria, properties);
  }
  var _attributeQueryExpression = null;
    //Messages
  var _attributeQueryExpressionNotInitializedMessage = "SDK.Metadata.Query.AttributeQueryExpression is not initialized.";
  var _invalidCriteriaMessage = "SDK.Metadata.Query.AttributeQueryExpression criteria must be an SDK.Metadata.Query.MetadataFilterExpression";
  var _invalidPropertiesMessage = "SDK.Metadata.Query.AttributeQueryExpression properties must be an SDK.Metadata.Query.MetadataPropertiesExpression";
  var _attributeQueryExpressionCriteriaAndPropertiesRequiredMessage = "SDK.Metadata.Query.AttributeQueryExpression criteria and properties parameter values are required."


  function _setValidCriteria(value) {
   try {
    _attributeQueryExpression.setCriteria(value);
   }
   catch (e) {
    throw e;
   }
  }
  function _setValidProperties(value) {
   try {
    _attributeQueryExpression.setProperties(value);
   }
   catch (e) {
    throw e;
   }
  }
  //Set parameter values
  if (typeof criteria == "undefined" || typeof properties == "undefined" || criteria == null || properties == null) {
   throw new Error(_attributeQueryExpressionCriteriaAndPropertiesRequiredMessage);
  }
  else {
   if (!(criteria instanceof SDK.Metadata.Query.MetadataFilterExpression)) {
    throw new Error(_invalidCriteriaMessage);
   }
   if (!(properties instanceof SDK.Metadata.Query.MetadataPropertiesExpression)) {
    throw new Error(_invalidPropertiesMessage);
   }
   _attributeQueryExpression = new SDK.Metadata.Query.MetadataQueryExpression(criteria, properties);
  }


  this.getCriteria = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataFilterExpression used in this SDK.Metadata.Query.AttributeQueryExpression instance.
  ///</summary>
  return _attributeQueryExpression.getCriteria(); };
  this.setCriteria = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.MetadataFilterExpression used in this SDK.Metadata.Query.AttributeQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.MetadataFilterExpression">
  /// The filter criteria to determine which attributes to return
  ///</param>
   _setValidCriteria(value);
  };
  this.getProperties = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataPropertiesExpression used in this SDK.Metadata.Query.AttributeQueryExpression instance.
  ///</summary>
  return _attributeQueryExpression.getProperties(); };
  this.setProperties = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.MetadataPropertiesExpression used in this SDK.Metadata.Query.AttributeQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.MetadataPropertiesExpression">
  /// The definition of the attribute properties to be returned.
  ///</param>
   _setValidProperties(value);
  };
  this._toXml = function () {
		///<summary>
  /// For internal use only.
  ///</summary>
   if (_attributeQueryExpression == null) {
    throw new Error(_attributeQueryExpressionNotInitializedMessage);
   }
   return ["<c:AttributeQuery>",
		              _attributeQueryExpression._toXml(),
		            "</c:AttributeQuery>"].join("");

  };




 };
 this.AttributeQueryExpression.__class = true;
 // SDK.Metadata.Query.AttributeQueryExpression END

 // SDK.Metadata.Query.Collection START
 this.Collection = function (type, list) {
  ///<summary>
  /// For internal use only.
  ///</summary>
  if (!(this instanceof SDK.Metadata.Query.Collection)) {
   return new SDK.Metadata.Query.Collection(type, list);
  }
  var _type = type;
  var _typeErrorMessage = "The value being added to the SDK.Metadata.Query.Collection is not the expected type.";
  var _objects = [];
  this.count = 0;

  this.add = function (item) {
  ///<summary>
  /// Adds an item to the collection.
  ///</summary>
  ///<param name="item" type="Object">
  /// The type of item must match the type defined for the collection.
  ///</param>
   if (_type == String) {
    item = new String(item);
   }
   if (_type == Number) {
    item = new Number(item);
   }
   if (item instanceof _type) {
    _objects.push(item);
    this.count++;
   }
   else
   { throw new Error(_typeErrorMessage) }


  };
  this.clear = function () {
  ///<summary>
  /// Removes all items from the collection
  ///</summary>
   _objects.length = 0;
   this.Count = 0;
  };
  this.remove = function (item) {
  ///<summary>
  /// Removes an item from the collection
  ///</summary>
  ///<param name="item" type="Object">
  /// The item must be a reference to the same object.
  ///</param>
   for (var i = 0; i < _objects.length; i++) {
    if (item === _objects[i]) {
     _objects.splice(i, 1);
     this.count--;
     return;
    }
   }
   throw new Error("Item '" + item.toString() + "' not found.");
  };
  this.contains = function (item) {
  ///<summary>
  /// Returns whether an object exists within the collection.
  ///</summary>
  ///<param name="item" type="Object">
  ///  The item must be a reference to the same object.
  ///</param>
   for (var i = 0; i < _objects.length; i++) {
    if (item === _objects[i]) {
     return true;
    }
   }
   return false;
  };
  this.addRange = function (items) {
  ///<summary>
  /// Adds an array of objects to the collection
  ///</summary>
  ///<param name="items" type="Array">
  /// Each item in the array must be the expected type for the collection.
  ///</param>
   var errorMessage = "SDK.Metadata.Query.Collection.addRange requires an array parameter.";
   if (items != null) {
    if (typeof items.push != "undefined")//Verify it is an array
    {
     for (var i = 0; i < items.length; i++) {
      this.add(items[i]);
     }
    }
    else {
     throw new Error(errorMessage);
    }
   }
   else {
    throw new Error(errorMessage);
   }

  };
  this.forEach = function (fn) {
  ///<summary>
  /// Applies the action contained within a delegate function.
  ///</summary>
  ///<param name="fn" type="Function">
  /// Delegate function with parameters for item and index.
  ///</param>
   for (var i = 0; i < _objects.length; i++) {
    fn(_objects[i], i);
   }
  };

  if (list != null) {
   this.addRange(list);
  }



 };
 this.Collection.__class = true;
 // SDK.Metadata.Query.Collection END

 // SDK.Metadata.Query.EntityQueryExpression START
	 this.EntityQueryExpression = function (criteria, properties, attributeQuery, relationshipQuery, labelQuery) {
   ///<summary>
  /// Defines a complex query to retrieve entity metadata. 
  ///</summary>
  ///<param name="criteria" type="SDK.Metadata.Query.MetadataFilterExpression">
  /// The filter criteria for the metadata query
  ///</param>
  ///<param name="properties" type="SDK.Metadata.Query.MetadataPropertiesExpression">
  /// The properties to be returned by the query
  ///</param>
  ///<param name="attributeQuery" type="SDK.Metadata.Query.AttributeQueryExpression">
  /// A query expression for the entity attribute metadata to return.
  ///</param>
  ///<param name="relationshipQuery" type="SDK.Metadata.Query.RelationshipQueryExpression ">
  /// A query expression for the entity relationship metadata to return
  ///</param>
  ///<param name="labelQuery" type="SDK.Metadata.Query.LabelQueryExpression ">
  /// A query expression for the labels to return.
  ///</param>
  if (!(this instanceof SDK.Metadata.Query.EntityQueryExpression)) {
   return new SDK.Metadata.Query.EntityQueryExpression(criteria, properties, attributeQuery, relationshipQuery, labelQuery);
  }
  //Properties
  var _entityQueryExpression = null;
  var _attributeQuery = null;
  var _relationshipQuery = null;
  var _labelQuery = null;

    //Messages
  var _notInitializedMessage = "SDK.Metadata.Query.EntityQueryExpression is not initialized.";
  var _invalidCriteriaMessage = "SDK.Metadata.Query.EntityQueryExpression criteria must be an SDK.Metadata.Query.MetadataFilterExpression";
  var _invalidPropertiesMessage = "SDK.Metadata.Query.EntityQueryExpression properties must be an SDK.Metadata.Query.MetadataPropertiesExpression";
  var _invalidAttributeQueryMessage = "SDK.Metadata.Query.EntityQueryExpression attributeQuery must be an SDK.Metadata.Query.AttributeQueryExpression";
  var _invalidLabelQueryMessage = "SDK.Metadata.Query.EntityQueryExpression labelQuery must be an SDK.Metadata.Query.LabelQueryExpression";
  var _invalidRelationshipQueryMessage = "SDK.Metadata.Query.EntityQueryExpression relationshipQuery must be an SDK.Metadata.Query.RelationshipQueryExpression";
  var _criteriaAndPropertiesRequiredMessage = "SDK.Metadata.Query.EntityQueryExpression criteria and properties parameter values are required.";

  // Internal functions
  function _setValidCriteria(value) {
   try {
    _entityQueryExpression.setCriteria(value);
   }
   catch (e) {
    throw e;
   }
  }
  function _setValidProperties(value) {
   try {
    _entityQueryExpression.setProperties(value);
   }
   catch (e) {
    throw e;
   }
  }
  function _setValidAttributeQuery(value) {
   if (value == null)
   { _attributeQuery = value; }
   else {
    if (value instanceof SDK.Metadata.Query.AttributeQueryExpression)
    { _attributeQuery = value; }
    else {
     throw new Error(_invalidAttributeQueryMessage);
    }
   }

  }
  function _setValidRelationshipQuery(value) {
   if (value == null)
   { _relationshipQuery = value; }
   else {
    if (value instanceof SDK.Metadata.Query.RelationshipQueryExpression)
    { _relationshipQuery = value; }
    else {
     throw new Error(_invalidRelationshipQueryMessage);
    }
   }

  }
  function _setValidLabelQuery(value) {
   if (value == null)
   { _labelQuery = value; }
   else {
    if (value instanceof SDK.Metadata.Query.LabelQueryExpression)
    { _labelQuery = value; }
    else {
     throw new Error(_invalidLabelQueryMessage);
    }
   }

  }

  //Set parameter values
  if (typeof criteria == "undefined" || typeof properties == "undefined" || criteria == null || properties == null) {
   throw new Error(_criteriaAndPropertiesRequiredMessage);
  }
  else {
   try {
    _entityQueryExpression = new SDK.Metadata.Query.MetadataQueryExpression(criteria, properties);
   }
   catch (e) {
    throw e;
   }
  }

  if (typeof attributeQuery != "undefined") {
   _setValidAttributeQuery(attributeQuery);
  }

  if (typeof relationshipQuery != "undefined") {
   _setValidRelationshipQuery(relationshipQuery);
  }

  if (typeof labelQuery != "undefined") {
   _setValidLabelQuery(labelQuery);
  }


  this.getCriteria = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataFilterExpression used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  return _entityQueryExpression.getCriteria(); };
  this.setCriteria = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.MetadataFilterExpression used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.MetadataFilterExpression">
  /// The filter criteria to determine which entities to return
  ///</param>
   _setValidCriteria(value);
  };
  this.getProperties = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataPropertiesExpression used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  return _entityQueryExpression.getProperties(); };
  this.setProperties = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.MetadataPropertiesExpression used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.MetadataPropertiesExpression">
  /// The definition of the entity properties to be returned.
  ///</param>
   _setValidProperties(value);
  };
  this.getAttributeQuery = function () {
  ///<summary>
  /// Gets the SDK.Metadata.Query.AttributeQueryExpression used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
   return _attributeQuery; };
  this.setAttributeQuery = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.AttributeQueryExpression used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.AttributeQueryExpression">
  /// Describes which entity attributes and what properties of those attributes to return.
  ///</param>
   _setValidAttributeQuery(value);
  };
  this.getRelationshipQuery = function () {
  ///<summary>
  /// Gets the SDK.Metadata.Query.RelationshipQueryExpression  used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
   return _relationshipQuery; };
  this.setRelationshipQuery = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.RelationshipQueryExpression  used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.RelationshipQueryExpression">
  /// Describes which entity relationships and what properties of those relationships to return.
  ///</param>
   _setValidRelationshipQuery(value);
  };
  this.getLabelQuery = function () {
  ///<summary>
  /// Gets the SDK.Metadata.Query.LabelQueryExpression   used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  ///<returns></returns>
   return _labelQuery; };
  this.setLabelQuery = function (value) {
  ///<summary>
  /// Gets the SDK.Metadata.Query.LabelQueryExpression used in this SDK.Metadata.Query.EntityQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.LabelQueryExpression">
  /// The definition of the language for labels to be returned.
  ///</param>
   _setValidLabelQuery(value);
  };
  this._toXml = function () {
		///<summary>
  /// For internal use only.
  ///</summary>
   if (_entityQueryExpression == null) {
    throw new Error(_notInitializedMessage);
   }
   var _attributeQueryXML = "<c:AttributeQuery i:nil=\"true\" />";
   if (_attributeQuery != null) {
    _attributeQueryXML = _attributeQuery._toXml();
   }
   var _labelQueryXML = "<c:LabelQuery i:nil=\"true\" />";
   if (_labelQuery != null) {
    _labelQueryXML = _labelQuery._toXml();
   }
   var _relationshipQueryXML = "<c:RelationshipQuery i:nil=\"true\" />";
   if (_relationshipQuery != null) {
    _relationshipQueryXML = _relationshipQuery._toXml();
   }

   return ["<b:value i:type=\"c:EntityQueryExpression\" xmlns:c=\"http://schemas.microsoft.com/xrm/2011/Metadata/Query\">",
		              _entityQueryExpression._toXml(),
		              _attributeQueryXML,
		              _labelQueryXML,
		              _relationshipQueryXML,
		            "</b:value>"].join("");

  };



 };
 this.EntityQueryExpression.__class = true;

 // SDK.Metadata.Query.EntityQueryExpression END

 // SDK.Metadata.Query.GuidValue START
	 //Use when setting a condition value that is a GUID otherwise you get an error like:
 // 'Error: -2147204733 Property MetadataId of type System.Nullable`1[System.Guid] cannot be compared with a condition value of type System.String'
 //i.e.: new SDK.Metadata.Query.MetadataConditionExpression("MetadataId", SDK.Metadata.Query.MetadataConditionOperator.Equals, new SDK.Metadata.Query.GuidValue(entityMetadataId));
 this.GuidValue = function (stringValue) {
   ///<summary>
  /// Required to use when setting a condition value that is a GUID
  ///</summary>
  ///<param name="stringValue" type="String">
  /// A string representation of a GUID value such as 'EF9894C9-44B1-49E9-95F2-856B78C7B4A4'
  ///</param>
  if (!(this instanceof SDK.Metadata.Query.GuidValue)) {
   return new SDK.Metadata.Query.GuidValue(stringValue);
  }
  var _value = null;
  if (/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(stringValue)) {
   _value = stringValue;
  }
  else {
   throw new Error("Value passed as a Guid Value is not a valid Guid.");
  }

  this.value = _value;

 };
 this.GuidValue.__class = true;
 // SDK.Metadata.Query.GuidValue END

 // SDK.Metadata.Query.LabelQueryExpression START
	 this.LabelQueryExpression = function (languages) {
   ///<summary>
  /// Defines the languages for the labels to be retrieved for metadata items that have labels. 
  ///</summary>
  ///<param name="languages" type="Array">
  /// An array of LCID number values
  ///</param>

  if (!(this instanceof SDK.Metadata.Query.LabelQueryExpression)) {
   return new SDK.Metadata.Query.LabelQueryExpression(languages);
  }

  var _filterLanguages = null;
  var _toXml
  //Set parameter values
  if (typeof languages != "undefined") {
   try {
    _filterLanguages = new SDK.Metadata.Query.Collection(Number, languages);
   }
   catch (e)
	{ throw e }

  }
  else {
   _filterLanguages = new SDK.Metadata.Query.Collection(Number);
  }

  this.filterLanguages = _filterLanguages;
  this._toXml = function () {
		///<summary>
  /// For internal use only.
  ///</summary>
   if (_filterLanguages == null) {
    throw new Error("SDK.Metadata.Query.LabelQueryExpression is not initialized.");
   }
   var languages = [];
   _filterLanguages.forEach(function (ln, i) {
    languages.push("<d:int>" + ln + "</d:int>");
   })

   return ["<c:FilterLanguages xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">",
											languages.join(""),
										"</c:FilterLanguages>"].join("");

  };

 };
 this.LabelQueryExpression.__class = true;
 // SDK.Metadata.Query.LabelQueryExpression END

 // SDK.Metadata.Query.MetadataConditionExpression START
	 this.MetadataConditionExpression = function (propertyName, conditionOperator, value) {
  ///<summary>
  /// Contains a condition expression used to filter the results of the metadata query. 
  ///</summary>
  ///<param name="propertyName" type="String">
  /// <para>The metadata property to evaluate</para>
  /// <para>Optional: Use one of the following enumerations depending on the type of metadata item:</para>
  /// <para> Entity: SDK.Metadata.Query.SearchableEntityMetadataProperties</para>
  /// <para> Attribute: SDK.Metadata.Query.SearchableAttributeMetadataProperties</para>
  /// <para> Relationship: SDK.Metadata.Query.SearchableRelationshipMetadataProperties</para>
  ///</param>
  ///<param name="conditionOperator" type="SDK.Metadata.Query.MetadataConditionOperator">
  /// The condition operator
  ///</param>
  ///<param name="value" type="String">
  /// <para>The metadata value to evaluate</para>
  /// <para>If the value is a GUID you must use SDK.Metadata.Query.GuidValue</para>
  /// <para>Optional: Use one of the SDK.Metadata.Query.ValueEnums enumerations depending on the type of metadata item:</para>
  /// <para> Entity.OwnershipType: SDK.Metadata.Query.ValueEnums.OwnershipType</para>
  /// <para> Attribute.AttributeType : SDK.Metadata.Query.ValueEnums.AttributeTypeCode</para>
  /// <para> Attribute.RequiredLevel : SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel</para>
  /// <para> DateTimeAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.DateTimeFormat</para>
  /// <para> IntegerAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.IntegerFormat</para>
  /// <para> StringAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.StringFormat</para>
  /// <para> Attribute.ImeMode SDK.Metadata.Query.ValueEnums.ImeMode</para>
  /// <para> Relationship.SecurityTypes: SDK.Metadata.Query.ValueEnums.SecurityTypes</para>
  ///</param>
  if (!(this instanceof SDK.Metadata.Query.MetadataConditionExpression)) {
   return new SDK.Metadata.Query.MetadataConditionExpression(propertyName, conditionOperator, value);
  }
  //Properties
  var _conditionOperator = null;
  var _propertyName = null;
  var _value = null;
  var _valueType = null;
  var _valueIsArray = false;
  var _valueNamespaces = {
   Serialization: "\"http://schemas.microsoft.com/2003/10/Serialization/\"",
   Arrays: "\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"",
   Metadata: "\"http://schemas.microsoft.com/xrm/2011/Metadata\"",
   XMLSchema: "\"http://www.w3.org/2001/XMLSchema\""
  }
  var _valueNamespace = null;

    //Messages
  var _nullPropertiesMessage = "A SDK.Metadata.Query.MetadataConditionExpression has properties that are null."
  var _invalidPropertyNameMessage = "The propertyName parameter value must be a string.";
  var _invalidConditionOperatorMessage = "The conditionOperator parameter value must be an SDK.Metadata.Query.MetadataConditionOperator";
  var _invalidValueMessage = "The value parameter must be either an array, a string, a number, a boolean value, or null. If an array, it must have at least one item.";
  var _propertyMustBeKnownToSetValueMessage = "The MetadataConditionExpression property name must be known to validate the MetadataConditionExpression value.";

  //Internal Functions
  function _getValueType(value) {
   if (_propertyName == null)
    throw new Error(_propertyMustBeKnownToSetValueMessage);

   var isMetadataEnum = false;
   var metadataEnumPropertyValues = ["AttributeType", "RelationshipType", "OwnershipType", "RequiredLevel", "Format", "ImeMode", "SecurityTypes"];
   for (var i = 0; i < metadataEnumPropertyValues.length; i++) {
    if (_propertyName == metadataEnumPropertyValues[i]) {
     isMetadataEnum = true;
     _valueNamespace = _valueNamespaces.Metadata;
     break;
    }
   }
   if (isMetadataEnum) {
    switch (_propertyName) {
     case "AttributeType":
      _valueType = "AttributeTypeCode";
      break;
      case "RelationshipType":
      _valueType = "RelationshipType";
      break;
     case "OwnershipType":
      _valueType = "OwnershipTypes";
      break;
     case "RequiredLevel":
      _valueType = "AttributeRequiredLevel";
      break;
     case "Format":

      var isDatetime = false;
      var isInteger = false;
      var isString = false;
      //Without knowing what type of attribute, we can only determine the correct valueString by looking at the values.
      //DateTimeFormat is only valid for DateTime attributes
      for (var i in SDK.Metadata.Query.ValueEnums.DateTimeFormat) {
       if (_value == i) {
        _valueType = "DateTimeFormat";
        isDatetime = true;
        break;
       }
      }
      //IntegerFormat is only valid for Integer attributes.
      if (!isDatetime) {
       for (var i in SDK.Metadata.Query.ValueEnums.IntegerFormat) {
        if (_value == i) {
         isInteger = true;
         _valueType = "IntegerFormat";
         break;
        }
       }
      }
      //StringFormat is only valid for String attributes.
      if (!isDatetime && !isInteger) {
       for (var i in SDK.Metadata.Query.ValueEnums.StringFormat) {
        if (_value == i) {
         isString = true;
         _valueType = "StringFormat";
         break;
        }
       }
      }
      if (!isDatetime && !isInteger && !isString) {
       throw new Error("Unexpected attribute Format metadata enumeration."); //Should not happen, but...
      }
      break;
     case "ImeMode":
      _valueType = "ImeMode";
      break;
     case "SecurityTypes":
      _valueType = "SecurityTypes";
      break;
    }
   }
   else {

    if (typeof value == "string" || typeof value == "number" || typeof value == "boolean") {
     if (typeof value == "number")
     { _valueType = "int" }
     else {
      _valueType = typeof value;
     }
    }

   }

   _valueIsArray = (typeof value == "object" && typeof value.push == 'function');
   if (_valueIsArray && !isMetadataEnum) {
    _valueNamespace = _valueNamespaces.Arrays;

    if (value.length > 0) {
     var firstItemType = typeof value[0];

     if (firstItemType == "string" || firstItemType == "number" || firstItemType == "boolean") {
      if (firstItemType == "number")
      { _valueType = "int" }
      else {
       _valueType = firstItemType;
      }
     }
     else
     { throw new Error("Cannot determine the type of items in an array passed as a value to a SDK.Metadata.Query.MetadataConditionExpression."); }

    }
    else {
     //This shouldn't happen because _setValidValue tests whether an array has no items;
     throw new Error(_invalidValueMessage);
    }

   }
   if (!_valueIsArray && !isMetadataEnum) {
    _valueNamespace = _valueNamespaces.XMLSchema;
   }
  }

  function _setValidPropertyName(value) {
   if (typeof value == "string") {
    _propertyName = value;
    if (_value != null) {
     _getValueType(_value); //Value variables have a dependency on _propertyName
    }

   }
   else {
    throw new Error(_invalidPropertyNameMessage);
   }
  }

  function _isValidConditionOperator(value) {
   for (var property in SDK.Metadata.Query.MetadataConditionOperator) {
    if (value == property)
     return true;
   }
   return false;
  }

  function _setValidConditionOperator(value) {
   if (value == null) {
    _conditionOperator = value;
   }
   else {
    if (_isValidConditionOperator(value)) {
     _conditionOperator = value;
    }
    else {
     throw new Error(_invalidConditionOperatorMessage);
    }
   }
  }

  function _setValidValue(value) {
     if (value == null)
   {
    _value = null;
    _valueType = "null";
   }
   else
   {
     if (value instanceof SDK.Metadata.Query.GuidValue) {
    _value = value.value;
    _valueType = "guid";
    _valueNamespace = _valueNamespaces.Serialization;
   }

   else {

    if ((typeof value == "undefined" || value == null) ||
			(typeof value != "string" && typeof value != "number" && typeof value != "boolean" && typeof value != "object") ||
			(typeof value == "object" && typeof value.push == "undefined") ||
			(typeof value == "object" && typeof value.push == "function" && value.length == 0)) 
   {

     throw new Error(_invalidValueMessage);
    }


    _getValueType(value);
    _value = value;
   }
   }





  }

  //Set parameter values
  if (typeof propertyName != undefined)
   _setValidPropertyName(propertyName);
  if (typeof conditionOperator != undefined)
   _setValidConditionOperator(conditionOperator);
  if (typeof value != undefined)
   _setValidValue(value);

  //Public Methods
  this.getConditionOperator = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataConditionOperator used in this SDK.Metadata.Query.MetadataConditionExpression  instance.
  ///</summary>
  return _conditionOperator; };
  this.setConditionOperator = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.MetadataConditionOperator used in this SDK.Metadata.Query.MetadataConditionExpression  instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.MetadataConditionOperator">
  /// Describes the type of comparison for two values in a metadata condition expression. 
  ///</param>
   _setValidConditionOperator(value);
  };
  this.getPropertyName = function () {
  ///<summary>
  /// Gets the name of the metadata property in the condition expression. 
  ///</summary>
   return _propertyName; };
  this.setPropertyName = function (value) {
  ///<summary>
  /// Sets the name of the metadata property in the condition expression. 
  ///</summary>
  ///<param name="value" type="String">
  /// <para>The metadata property to evaluate</para>
  /// <para>Optional: Use one of the following enumerations depending on the type of metadata item:</para>
  /// <para> Entity: SDK.Metadata.Query.SearchableEntityMetadataProperties</para>
  /// <para> Attribute: SDK.Metadata.Query.SearchableAttributeMetadataProperties</para>
  /// <para> Relationship: SDK.Metadata.Query.SearchableRelationshipMetadataProperties</para>
  ///</param>
   _setValidPropertyName(value);
  };
  this.getValue = function () { 
  ///<summary>
  /// Gets the value to compare.
  ///</summary>

  return _value; };
  this.setValue = function (value) {
  ///<summary>
  /// Sets the value to compare.
  ///</summary>
  ///<param name="value" type="String">
  /// <para>The metadata value to evaluate</para>
  /// <para>If the value is a GUID you must use SDK.Metadata.Query.GuidValue</para>
  /// <para>Optional: Use one of the SDK.Metadata.Query.ValueEnums enumerations depending on the type of metadata item:</para>
  /// <para> Entity.OwnershipType: SDK.Metadata.Query.ValueEnums.OwnershipType</para>
  /// <para> Attribute.AttributeType : SDK.Metadata.Query.ValueEnums.AttributeTypeCode</para>
  /// <para> Attribute.RequiredLevel : SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel</para>
  /// <para> DateTimeAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.DateTimeFormat</para>
  /// <para> IntegerAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.IntegerFormat</para>
  /// <para> StringAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.StringFormat</para>
  /// <para> Attribute.ImeMode SDK.Metadata.Query.ValueEnums.ImeMode</para>
  /// <para> Relationship.SecurityTypes: SDK.Metadata.Query.ValueEnums.SecurityTypes</para>
  ///</param>
   _setValidValue(value);
  };
  this._toXml = function () {
		///<summary>
  /// For internal use only.
  ///</summary>
   if (_conditionOperator == null ||
			_propertyName == null ||
			_valueType == null) {
    throw new Error(_nullPropertiesMessage);
   }

   var valueString;
			if (_valueType == "null")
   {
   valueString = "<c:Value i:nil=\"true\" />";
   }
			else {
			if (_valueIsArray) {
    valueString = "<c:Value i:type=\"d:ArrayOf" + _valueType + "\" xmlns:d=" + _valueNamespace + ">";
    for (var i = 0; i < _value.length; i++) {
     valueString += "<d:" + _valueType + ">" + _value[i] + "</d:" + _valueType + ">";
    }
    valueString += "</c:Value>";
   }
			else
			{
			 valueString = "<c:Value i:type=\"d:" + _valueType + "\" xmlns:d=" + _valueNamespace + ">" + _value + "</c:Value>";
			}
   
   }
   

   


   return ["<c:MetadataConditionExpression>",
																	"<c:ConditionOperator>" + _conditionOperator + "</c:ConditionOperator>",
																	"<c:PropertyName>" + _propertyName + "</c:PropertyName>",
																	valueString,
															"</c:MetadataConditionExpression>"].join("");
  };




 }
 this.MetadataConditionExpression.__class = true;

 // SDK.Metadata.Query.MetadataConditionExpression END

 // SDK.Metadata.Query.MetadataFilterExpression START
	 this.MetadataFilterExpression = function (filterOperator) {
  ///<summary>
  /// Specifies complex condition and logical filter expressions used for filtering the results of a metadata query. 
  ///</summary>
  ///<param name="filterOperator" type="SDK.Metadata.Query.LogicalOperator">
  /// The logical AND/OR filter operator.
  ///</param>

  if (!(this instanceof SDK.Metadata.Query.MetadataFilterExpression)) {
   return new SDK.Metadata.Query.MetadataFilterExpression(filterOperator);
  }

  var _filterOperator = SDK.Metadata.Query.LogicalOperator.And; //Default value
  var _conditionExpressions = new SDK.Metadata.Query.Collection(SDK.Metadata.Query.MetadataConditionExpression);
  var _filterExpressions = new SDK.Metadata.Query.Collection(SDK.Metadata.Query.MetadataFilterExpression);

    //Messages
  var _invalidFilterOperatorMessage = "SDK.Metadata.Query.MetadataFilterExpression FilterOperator requires a SDK.Metadata.Query.LogicalOperator.";
  var _invalidconditionExpressionsMessage = "SDK.Metadata.Query.MetadataFilterExpression Conditions requires a SDK.Metadata.Query.MetadataConditionExpressionCollection.";
		

  function _toXml() {
   var conditions = [];
   _conditionExpressions.forEach(function (mce, i) {
    conditions.push(mce._toXml());
   })

   var filters = [];
   _filterExpressions.forEach(function (mfe, i) {
				filters.push("<c:MetadataFilterExpression>");
    filters.push(mfe._toXml());
				filters.push("</c:MetadataFilterExpression>");
   })

   return ["<c:Conditions>",
											conditions.join(""),
		          "</c:Conditions>",
		          "<c:FilterOperator>" + _filterOperator + "</c:FilterOperator>",
		          "<c:Filters>",
												filters.join(""),
												"</c:Filters>",
		        ].join("");
  };


  function _setValidConditions(value) {
   if (value instanceof SDK.Metadata.Query.MetadataConditionExpressionCollection) {
    _conditionExpressions = value;
   }
   else {
    throw new Error(_invalidconditionExpressionsMessage);
   }

  }

  function _setValidFilterOperator(value) {
   if ((value != null) && (value == "Or" || value == "And")) {
    _filterOperator = value;
   }
   else {
    throw new Error(_invalidFilterOperatorMessage);
   }

  }

  //Set parameter values
  if (typeof filterOperator != "undefined") {
   _setValidFilterOperator(filterOperator);
  }

	
		this.addCondition = function(propertyName, conditionOperator, value){
		///<summary>
  /// <para>Adds a condition. This method accepts either the properties to create a new SDK.Metadata.Query.MetadataConditionExpression </para>
  /// <para>or a SDK.Metadata.Query.MetadataConditionExpression as the first argument.</para>
  ///</summary>
  ///<param name="propertyName" type="String">
  /// <para>The metadata property to evaluate</para>
  /// <para>Optional: Use one of the following enumerations depending on the type of metadata item:</para>
  /// <para> Entity: SDK.Metadata.Query.SearchableEntityMetadataProperties</para>
  /// <para> Attribute: SDK.Metadata.Query.SearchableAttributeMetadataProperties</para>
  /// <para> Relationship: SDK.Metadata.Query.SearchableRelationshipMetadataProperties</para>
  ///</param>
  ///<param name="conditionOperator" type="SDK.Metadata.Query.MetadataConditionOperator">
  /// The condition operator
  ///</param>
  ///<param name="value" type="Object">
  /// <para>The metadata value to evaluate</para>
  /// <para>If the value is a GUID you must use SDK.Metadata.Query.GuidValue</para>
  /// <para>Optional: Use one of the SDK.Metadata.Query.ValueEnums enumerations depending on the type of metadata item:</para>
  /// <para> Entity.OwnershipType: SDK.Metadata.Query.ValueEnums.OwnershipType</para>
  /// <para> Attribute.AttributeType : SDK.Metadata.Query.ValueEnums.AttributeTypeCode</para>
  /// <para> Attribute.RequiredLevel : SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel</para>
  /// <para> DateTimeAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.DateTimeFormat</para>
  /// <para> IntegerAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.IntegerFormat</para>
  /// <para> StringAttributeMetadata.Format SDK.Metadata.Query.ValueEnums.StringFormat</para>
  /// <para> Attribute.ImeMode SDK.Metadata.Query.ValueEnums.ImeMode</para>
  /// <para> Relationship.SecurityTypes: SDK.Metadata.Query.ValueEnums.SecurityTypes</para>
  ///</param>
  if ((propertyName instanceof SDK.Metadata.Query.MetadataConditionExpression) && (conditionOperator == null) && (value == null))
  {
   _conditionExpressions.add(propertyName);
  }
  else
  {
  		_conditionExpressions.add(new SDK.Metadata.Query.MetadataConditionExpression(propertyName, conditionOperator, value));
  }

		}
  this.addConditions = function(conditions){
  ///<summary>
  /// Adds an array of conditions.
  ///</summary>
  ///<param name="conditions" type="Array">
  /// An Array of SDK.Metadata.Query.MetadataConditionExpression objects
  ///</param>

  _conditionExpressions.addRange(conditions);
  }
		this.addFilter = function(filter){
		///<summary>
  /// Adds a filter
  ///</summary>
		///<param name="filter" type="SDK.Metadata.Query.MetadataFilterExpression">
  /// The condition to add.
  ///</param>
		if (!(filter instanceof SDK.Metadata.Query.MetadataFilterExpression)) {
  throw new Error("addFilter filter parameter requires a SDK.Metadata.Query.MetadataFilterExpression)");
  }
		_filterExpressions.add(filter);
		}


  this.getConditions = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.Collection of SDK.Metadata.Query.MetadataConditionExpression instances.
  ///</summary>
  return _conditionExpressions; 
  };
  //Note: setConditions is included for consistency since it is a get/set property.
  // But for normal usage people should use "getConditions().add(..." to add or remove  conditions.
  // Use setConditions only to overwrite the existing SDK.Metadata.Query.Collection
  this.setConditions = function (value) { 
  ///<summary>
  /// Sets the SDK.Metadata.Query.Collection of SDK.Metadata.Query.MetadataConditionExpression instances.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.Collection">
  /// <para>Sets the SDK.Metadata.Query.Collection of SDK.Metadata.Query.MetadataConditionExpression instances.</para>
  /// <para>Recommend you use getConditions().add(.. or getConditions().remove(.. to change the contitions.</para>
  ///</param>
  _setValidConditions(value); };
  this.getFilterOperator = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.LogicalOperator representing the FilterOperator for this SDK.Metadata.Query.MetadataFilterExpression.
  ///</summary>

  return _filterOperator; };
  this.setFilterOperator = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.LogicalOperator representing the FilterOperator for this SDK.Metadata.Query.MetadataFilterExpression.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.LogicalOperator">
  /// The SDK.Metadata.Query.LogicalOperator representing the FilterOperator for this SDK.Metadata.Query.MetadataFilterExpression.
  ///</param>
   _setValidFilterOperator(value);
  };

  this.getFilters = function () {
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataFilterExpression objects used as filters in this SDK.Metadata.Query.MetadataFilterExpression
  ///</summary>
  return _filterExpressions; };
  this._toXml = _toXml;





 };
 this.MetadataFilterExpression.__class = true;
 // SDK.Metadata.Query.MetadataFilterExpression END

 // SDK.Metadata.Query.MetadataPropertiesExpression START
	 this.MetadataPropertiesExpression = function (allProperties, propertyNames) {
  ///<summary>
  /// Specifies the properties for which non-null values are returned from a query. 
  ///</summary>
  ///<param name="allProperties" type="Boolean">
  /// Whether to retrieve all the properties of a metadata object.
  ///</param>
  ///<param name="propertyNames" type="Array">
  /// <para>An array of strings representing the metadata properties to retrieve.</para>
  /// <para>Optional: Use one of the following enumerations depending on the metadata item:</para>
  /// <para> Entity: SDK.Metadata.Query.EntityMetadataProperties</para>
  /// <para> Attribute: SDK.Metadata.Query.AttributeMetadataProperties</para>
  /// <para> Relationship: SDK.Metadata.Query.RelationshipMetadataProperties</para>
  ///</param>
  if (!(this instanceof SDK.Metadata.Query.MetadataPropertiesExpression)) {
   return new SDK.Metadata.Query.MetadataPropertiesExpression(allProperties, propertyNames);
  }
  //Properties
  var _allProperties = allProperties;
  var _propertyNames = new SDK.Metadata.Query.Collection(String);

    // Messages
  var _invalidAllPropertiesMessage = "The SDK.Metadata.Query.MetadataPropertiesExpression allProperties  must be an Boolean value.";
  var _invalidPropertyNamesMessage = "The SDK.Metadata.Query.MetadataPropertiesExpression propertyNames  must be an Array.";

  //Internal Functions
  function _setValidAllProperties(value) {
   if (typeof value == "boolean") {
    _allProperties = value;
   }
   else {
    throw new Error(_invalidAllPropertiesMessage);
   }
  }
  function _setValidPropertyNames(value) {
   if (typeof value.push != "undefined") //check if it is an array
   {
    _propertyNames = new SDK.Metadata.Query.Collection(String, value);
   }
   else {
    throw new Error(_invalidPropertyNamesMessage);
   }
  }

  //Set parameter values
  if (typeof allProperties != "undefined") {
   _setValidAllProperties(allProperties);
  }
  if (typeof propertyNames != "undefined") {
   _setValidPropertyNames(propertyNames);
  }

  //Public Methods
  this.addProperty = function(propertyName){
  ///<summary>
  /// Adds the property name to the properties to return.
  ///</summary>
  ///<param name="propertyName" type="String">
  ///<para> Use one of the following enumerations to set the property value:</para>
  ///<para>SDK.Metadata.Query.EntityMetadataProperties</para>
  ///<para>SDK.Metadata.Query.AttributeMetadataProperties</para>
  ///<para>SDK.Metadata.Query.RelationshipMetadataProperties</para>
  ///</param>
  _propertyNames.add(propertyName);
  };
  this.addProperties = function(propertyNames){
  ///<summary>
  /// Adds an array of properties
  ///</summary>
  ///<param name="propertyNames" type="Array">
  ///<para> Use one of the following enumerations to set string values in this array:</para>
  ///<para>SDK.Metadata.Query.EntityMetadataProperties</para>
  ///<para>SDK.Metadata.Query.AttributeMetadataProperties</para>
  ///<para>SDK.Metadata.Query.RelationshipMetadataProperties</para>
  ///</param>
  _propertyNames.addRange(propertyNames);
  };
  this.getAllProperties = function () { 
  ///<summary>
  /// Gets the boolean value indicating if all properties should be returned.
  ///</summary>

  return _allProperties; };
  this.setAllProperties = function (value) {
  ///<summary>
  /// Sets the value indicating if all properties should be returned.
  ///</summary>
  ///<param name="value" type="Boolean">
  /// The value indicating if all properties should be returned.
  ///</param>
   _setValidAllProperties(value);
  };
  this.getPropertyNames = function () { 
  ///<summary>
  /// Gets the Collection of property names to be returned.
  ///</summary>

  return _propertyNames; };
  this.setPropertyNames = function (value) {
  ///<summary>
  /// Sets the properties to be returned.
  ///</summary>
  ///<param name="value" type="Array">
  /// An array of string values fore the properties to be returned.
  ///</param>
   _setValidPropertyNames(value);
  };
  this._toXml = function () {
		///<summary>
  /// For internal use only.
  ///</summary>
   var properties = [];
   _propertyNames.forEach(function (pn, i) {
    properties.push("<d:string>" + pn + "</d:string>");
   })

   return [
		          "<c:AllProperties>" + _allProperties + "</c:AllProperties>",
		          "<c:PropertyNames xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">",
												properties.join(""),
												"</c:PropertyNames>"].join("");

  };



 };
 this.MetadataPropertiesExpression.__class = true;
 // SDK.Metadata.Query.MetadataPropertiesExpression END

 // SDK.Metadata.Query.MetadataQueryExpression START
	 // This is a base class to encapsulate shared functionality exposed by 
 // AttributeQueryExpression, EntityQueryExpression, and RelationshipQueryExpression
 this.MetadataQueryExpression = function (criteria, properties) {
  ///<summary>
  /// For internal use only.
  ///</summary>
  if (!(this instanceof SDK.Metadata.Query.MetadataQueryExpression)) {
   return new SDK.Metadata.Query.MetadataQueryExpression(criteria, properties);
  }

  var _criteria = null // SDK.Metadata.Query.MetadataFilterExpression
  var _properties = null // SDK.Metadata.Query.MetadataPropertiesExpression

    //Messages
  var _criteriaIsNullMessage = "SDK.Metadata.Query.MetadataQueryExpression criteria  is null.";
  var _PropertiesIsNullMessage = "SDK.Metadata.Query.MetadataQueryExpression properties  is null.";
  var _invalidCriteriaMessage = "SDK.Metadata.Query.MetadataQueryExpression criteria  must be an SDK.Metadata.Query.MetadataFilterExpression";
  var _invalidPropertiesMessage = "SDK.Metadata.Query.MetadataQueryExpression properties  must be an SDK.Metadata.Query.MetadataPropertiesExpression";

  function _setValidCriteria(value) {
   if (value == null)
   { _criteria = value; }
   else {
    if (value instanceof SDK.Metadata.Query.MetadataFilterExpression)
    { _criteria = value; }
    else
    { throw new Error(_invalidCriteriaMessage); }
   }

  }
  function _setValidProperties(value) {
   if (value == null) {
    _properties = value;
   }
   else {
    if (value instanceof SDK.Metadata.Query.MetadataPropertiesExpression)
    { _properties = value; }
    else
    { throw new Error(_invalidPropertiesMessage); }
   }

  }

  //Set parameter values
  _setValidCriteria(criteria);
  _setValidProperties(properties);


  this.getCriteria = function () { 
  ///<summary>
  /// For internal use only.
  ///</summary>

  return _criteria; };
  this.setCriteria = function (value) {
  ///<summary>
  /// For internal use only.
  ///</summary>
   _setValidCriteria(value);
  };
  this.getProperties = function () {
  ///<summary>
  /// For internal use only.
  ///</summary>

   return _properties; };
  this.setProperties = function (value) {
  ///<summary>
  /// For internal use only.
  ///</summary>
   _setValidProperties(value);
  };
  this._toXml = function () {
		///<summary>
  /// For internal use only.
  ///</summary>
   if (_criteria == null)
   { throw new Error(_criteriaIsNullMessage); }
   if (_properties == null)
   { throw new Error(_PropertiesIsNullMessage); }
   return ["<c:Criteria>",
										_criteria._toXml(),
										"</c:Criteria>",
										"<c:Properties>",
		        _properties._toXml(),
		        "</c:Properties>"].join("");
  };




 };
 this.MetadataQueryExpression.__class = true;
 // SDK.Metadata.Query.MetadataQueryExpression END

 // SDK.Metadata.Query.RelationshipQueryExpression START

	 this.RelationshipQueryExpression = function (criteria, properties) {
   ///<summary>
  /// Defines a complex query to retrieve entity relationship metadata for entities retrieved using an EntityQueryExpression
  ///</summary>
  ///<param name="criteria" type="SDK.Metadata.Query.MetadataFilterExpression">
  /// The filter criteria for the metadata query
  ///</param>
  ///<param name="properties" type="SDK.Metadata.Query.MetadataPropertiesExpression">
  /// The properties to be returned by the query
  ///</param>
  if (!(this instanceof SDK.Metadata.Query.RelationshipQueryExpression)) {
   return new SDK.Metadata.Query.RelationshipQueryExpression(criteria, properties);
  }

  var _relationshipQueryExpression = null;

    //Messages
  var _notInitializedMessage = "SDK.Metadata.Query.RelationshipQueryExpression is not initialized."
  var _invalidCriteriaMessage = "SDK.Metadata.Query.RelationshipQueryExpression criteria must be an SDK.Metadata.Query.MetadataFilterExpression";
  var _invalidPropertiesMessage = "SDK.Metadata.Query.RelationshipQueryExpression properties must be an SDK.Metadata.Query.MetadataPropertiesExpression";
  var _criteriaAndParametersRequiredMessage = "SDK.Metadata.Query.RelationshipQueryExpression criteria and properties parameter values are required."

  function _setValidCriteria(value) {
   try {
    _relationshipQueryExpression.setCriteria(value);
   }
   catch (e) {
    throw e;
   }
  }
  function _setValidProperties(value) {
   try {
    _relationshipQueryExpression.setProperties(value);
   }
   catch (e) {
    throw e;
   }
  }
  //Set parameter values
  if (typeof criteria == "undefined" || typeof properties == "undefined" || criteria == null || properties == null) {
   throw new Error(_criteriaAndParametersRequiredMessage);
  }
  else {
   if (!(criteria instanceof SDK.Metadata.Query.MetadataFilterExpression)) {
    throw new Error(_invalidCriteriaMessage);
   }
   if (!(properties instanceof SDK.Metadata.Query.MetadataPropertiesExpression)) {
    throw new Error(_invalidPropertiesMessage);
   }
   _relationshipQueryExpression = new SDK.Metadata.Query.MetadataQueryExpression(criteria, properties);
  }

  this.getCriteria = function () {
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataFilterExpression used in this SDK.Metadata.Query.RelationshipQueryExpression instance.
  ///</summary>

   return _relationshipQueryExpression.getCriteria(); };
  this.setCriteria = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.MetadataFilterExpression used in this SDK.Metadata.Query.RelationshipQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.MetadataFilterExpression">
  /// The filter criteria to determine which relationships to return
  ///</param>
   _setValidCriteria(value);
  };
  this.getProperties = function () { 
  ///<summary>
  /// Gets the SDK.Metadata.Query.MetadataPropertiesExpression used in this SDK.Metadata.Query.RelationshipQueryExpression instance.
  ///</summary>

  return _relationshipQueryExpression.getProperties(); };
  this.setProperties = function (value) {
  ///<summary>
  /// Sets the SDK.Metadata.Query.MetadataPropertiesExpression used in this SDK.Metadata.Query.RelationshipQueryExpression instance.
  ///</summary>
  ///<param name="value" type="SDK.Metadata.Query.MetadataPropertiesExpression">
  /// The definition of the relationship properties to be returned.
  ///</param>
   _setValidProperties(value);
  };
  this._toXml = function () {
		///<summary>
  /// For internal use only.
  ///</summary>
   if (_relationshipQueryExpression == null) {
    throw new Error(_notInitializedMessage);
   }
   return ["<c:RelationshipQuery>",
		              _relationshipQueryExpression._toXml(),
		            "</c:RelationshipQuery>"].join("");

  };



 };
 this.RelationshipQueryExpression.__class = true;

 // SDK.Metadata.Query.RelationshipQueryExpression END

 //Enumerations part 1 START
	 //These enumerations are defined as functions so that they provide an IntelliSense experience
 // in Visual Studio that is consistent with strongly typed languages like C#.
 // The actual enumeration are defined below.
 this.DeletedMetadataFilters = function () {
  /// <summary>An enumeration that specifies the type of deleted metadata to retrieve.</summary>
  /// <field name="All" type="Number" static="true">All deleted metadata</field>		
  /// <field name="Attribute" type="Number" static="true">Deleted Attribute metadata</field>
  /// <field name="Default" type="Number" static="true">The value used if not set. Equals Entity</field>
  /// <field name="Entity" type="Number" static="true">Deleted Entity metadata</field>
  /// <field name="Label" type="Number" static="true">Deleted Label metadata</field>
  /// <field name="OptionSet" type="Number" static="true">Deleted OptionSet metadata</field>
  /// <field name="Relationship" type="Number" static="true">Deleted Relationship metadata</field>
  
  

  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.MetadataConditionOperator = function () {
  /// <summary>SDK.Metadata.Query.MetadataConditionOperator enum summary</summary>
  /// <field name="Equals" type="String" static="true">enum field summary for Equals</field>	
  /// <field name="NotEquals" type="String" static="true">enum field summary for NotEquals</field>	
  /// <field name="In" type="String" static="true">enum field summary for In</field>	
  /// <field name="NotIn" type="String" static="true">enum field summary for NotIn</field>	
  /// <field name="GreaterThan" type="String" static="true">enum field summary for GreaterThan</field>	
  /// <field name="LessThan" type="String" static="true">enum field summary for LessThan</field>	
  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.LogicalOperator = function () {
  /// <summary>SDK.Metadata.Query.LogicalOperator enum summary</summary>
  /// <field name="And" type="String" static="true">enum field summary for And</field>
  /// <field name="Or" type="String" static="true">enum field summary for Or</field>
  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.SearchableEntityMetadataProperties = function () {
  /// <summary>SDK.Metadata.Query.SearchableEntityMetadataProperties enum summary</summary>
  /// <field name="ActivityTypeMask" type="String" static="true">enum field summary for ActivityTypeMask</field>	
  /// <field name="AutoRouteToOwnerQueue" type="String" static="true">enum field summary for AutoRouteToOwnerQueue</field>	
  /// <field name="CanBeInManyToMany" type="String" static="true">enum field summary for CanBeInManyToMany</field>	
  /// <field name="CanBePrimaryEntityInRelationship" type="String" static="true">enum field summary for CanBePrimaryEntityInRelationship</field>	
  /// <field name="CanBeRelatedEntityInRelationship" type="String" static="true">enum field summary for CanBeRelatedEntityInRelationship</field>	
  /// <field name="CanCreateAttributes" type="String" static="true">enum field summary for CanCreateAttributes</field>	
  /// <field name="CanCreateCharts" type="String" static="true">enum field summary for CanCreateCharts</field>	
  /// <field name="CanCreateForms" type="String" static="true">enum field summary for CanCreateForms</field>
  /// <field name="CanCreateViews" type="String" static="true">enum field summary for CanCreateViews</field>	
  /// <field name="CanModifyAdditionalSettings" type="String" static="true">enum field summary for CanModifyAdditionalSettings</field>
  /// <field name="CanTriggerWorkflow" type="String" static="true">enum field summary for CanTriggerWorkflow</field>	
  /// <field name="IconLargeName" type="String" static="true">enum field summary for IconLargeName</field>	
  /// <field name="IconMediumName" type="String" static="true">enum field summary for IconMediumName</field>	
  /// <field name="IconSmallName" type="String" static="true">enum field summary for IconSmallName</field>	
  /// <field name="IsActivity" type="String" static="true">enum field summary for IsActivity</field>	
  /// <field name="IsActivityParty" type="String" static="true">enum field summary for IsActivityParty</field>	
  /// <field name="IsAuditEnabled" type="String" static="true">enum field summary for IsAuditEnabled</field>	
  /// <field name="IsAvailableOffline" type="String" static="true">enum field summary for IsAvailableOffline</field>
  /// <field name="IsChildEntity" type="String" static="true">enum field summary for IsChildEntity</field>	
  /// <field name="IsConnectionsEnabled" type="String" static="true">enum field summary for IsConnectionsEnabled</field>
  /// <field name="IsCustomEntity" type="String" static="true">enum field summary for IsCustomEntity</field>	
  /// <field name="IsCustomizable" type="String" static="true">enum field summary for IsCustomizable</field>	
  /// <field name="IsDocumentManagementEnabled" type="String" static="true">enum field summary for IsDocumentManagementEnabled</field>	
  /// <field name="IsDuplicateDetectionEnabled" type="String" static="true">enum field summary for IsDuplicateDetectionEnabled</field>	
  /// <field name="IsEnabledForCharts" type="String" static="true">enum field summary for IsEnabledForCharts</field>	
  /// <field name="IsImportable" type="String" static="true">enum field summary for IsImportable</field>	
  /// <field name="IsIntersect" type="String" static="true">enum field summary for IsIntersect</field>	
  /// <field name="IsMailMergeEnabled" type="String" static="true">enum field summary for IsMailMergeEnabled</field>
  /// <field name="IsManaged" type="String" static="true">enum field summary for IsManaged</field>	
  /// <field name="IsMappable" type="String" static="true">enum field summary for IsMappable</field>
  /// <field name="IsReadingPaneEnabled" type="String" static="true">enum field summary for IsReadingPaneEnabled</field>	
  /// <field name="IsRenameable" type="String" static="true">enum field summary for IsRenameable</field>	
  /// <field name="IsValidForAdvancedFind" type="String" static="true">enum field summary for IsValidForAdvancedFind</field>	
  /// <field name="IsValidForQueue" type="String" static="true">enum field summary for IsValidForQueue</field>	
  /// <field name="IsVisibleInMobile" type="String" static="true">enum field summary for IsVisibleInMobile</field>	
  /// <field name="LogicalName" type="String" static="true">enum field summary for LogicalName</field>	
  /// <field name="MetadataId" type="String" static="true">enum field summary for MetadataId</field>	
  /// <field name="ObjectTypeCode" type="String" static="true">enum field summary for ObjectTypeCode</field>
  /// <field name="OwnershipType" type="String" static="true">enum field summary for OwnershipType</field>	
  /// <field name="PrimaryIdAttribute" type="String" static="true">enum field summary for PrimaryIdAttribute</field>
  /// <field name="PrimaryNameAttribute" type="String" static="true">enum field summary for PrimaryNameAttribute</field>	
  /// <field name="RecurrenceBaseEntityLogicalName" type="String" static="true">enum field summary for RecurrenceBaseEntityLogicalName</field>
  /// <field name="ReportViewName" type="String" static="true">enum field summary for ReportViewName</field>	
  /// <field name="SchemaName" type="String" static="true">enum field summary for SchemaName</field>
  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.SearchableAttributeMetadataProperties = function () {
  /// <summary>SDK.Metadata.Query.SearchableAttributeMetadataProperties enum summary</summary>
  /// <field name="AttributeOf" type="String" static="true">enum field summary for AttributeOf</field>	
  /// <field name="AttributeType" type="String" static="true">enum field summary for AttributeType</field>	
  /// <field name="CalculationOf" type="String" static="true">enum field summary for CalculationOf</field>	
  /// <field name="CanBeSecuredForCreate" type="String" static="true">enum field summary for CanBeSecuredForCreate</field>	
  /// <field name="CanBeSecuredForRead" type="String" static="true">enum field summary for CanBeSecuredForRead</field>	
  /// <field name="CanBeSecuredForUpdate" type="String" static="true">enum field summary for CanBeSecuredForUpdate</field>	
  /// <field name="CanModifyAdditionalSettings" type="String" static="true">enum field summary for CanModifyAdditionalSettings</field>	
  /// <field name="ColumnNumber" type="String" static="true">enum field summary for ColumnNumber</field>	
  /// <field name="DefaultFormValue" type="String" static="true">enum field summary for DefaultFormValue</field>	
  /// <field name="DefaultValue" type="String" static="true">enum field summary for DefaultValue</field>	
  /// <field name="DeprecatedVersion" type="String" static="true">enum field summary for DeprecatedVersion</field>	
  /// <field name="EntityLogicalName" type="String" static="true">enum field summary for EntityLogicalName</field>	
  /// <field name="Format" type="String" static="true">enum field summary for Format</field>	
  /// <field name="ImeMode" type="String" static="true">enum field summary for ImeMode</field>	
  /// <field name="IsAuditEnabled" type="String" static="true">enum field summary for IsAuditEnabled</field>	
  /// <field name="IsCustomAttribute" type="String" static="true">enum field summary for IsCustomAttribute</field>	
  /// <field name="IsCustomizable" type="String" static="true">enum field summary for IsCustomizable</field>	
  /// <field name="IsManaged" type="String" static="true">enum field summary for IsManaged</field>	
  /// <field name="IsPrimaryId" type="String" static="true">enum field summary for IsPrimaryId</field>	
  /// <field name="IsPrimaryName" type="String" static="true">enum field summary for IsPrimaryName</field>
  /// <field name="IsRenameable" type="String" static="true">enum field summary for IsRenameable</field>	
  /// <field name="IsSecured" type="String" static="true">enum field summary for IsSecured</field>	
  /// <field name="IsValidForAdvancedFind" type="String" static="true">enum field summary for IsValidForAdvancedFind</field>	
  /// <field name="IsValidForCreate" type="String" static="true">enum field summary for IsValidForCreate</field>	
  /// <field name="IsValidForRead" type="String" static="true">enum field summary for IsValidForRead</field>	
  /// <field name="IsValidForUpdate" type="String" static="true">enum field summary for IsValidForUpdate</field>	
  /// <field name="LinkedAttributeId" type="String" static="true">enum field summary for LinkedAttributeId</field>	
  /// <field name="LogicalName" type="String" static="true">enum field summary for LogicalName</field>	
  /// <field name="MaxLength" type="String" static="true">enum field summary for MaxLength</field>	
  /// <field name="MaxValue" type="String" static="true">enum field summary for MaxValue</field>
  /// <field name="MetadataId" type="String" static="true">enum field summary for MetadataId</field>	
  /// <field name="MinValue" type="String" static="true">enum field summary for MinValue</field>	
  /// <field name="Precision" type="String" static="true">enum field summary for Precision</field>	
  /// <field name="PrecisionSource" type="String" static="true">enum field summary for PrecisionSource</field>	
  /// <field name="RequiredLevel" type="String" static="true">enum field summary for RequiredLevel</field>	
  /// <field name="SchemaName" type="String" static="true">enum field summary for SchemaName</field>	
  /// <field name="YomiOf" type="String" static="true">enum field summary for YomiOf</field>	
  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.SearchableRelationshipMetadataProperties = function () {
  /// <summary>SDK.Metadata.Query.SearchableRelationshipMetadataProperties enum summary</summary>
  /// <field name="Entity1IntersectAttribute" type="String" static="true">enum field summary for Entity1IntersectAttribute</field>	
  /// <field name="Entity1LogicalName" type="String" static="true">enum field summary for Entity1LogicalName</field>	
  /// <field name="Entity2IntersectAttribute" type="String" static="true">enum field summary for Entity2IntersectAttribute</field>	
  /// <field name="Entity2LogicalName" type="String" static="true">enum field summary for Entity2LogicalName</field>	
  /// <field name="IntersectEntityName" type="String" static="true">enum field summary for IntersectEntityName</field>	
  /// <field name="IsCustomizable" type="String" static="true">enum field summary for IsCustomizable</field>	
  /// <field name="IsCustomRelationship" type="String" static="true">enum field summary for IsCustomRelationship</field>	
  /// <field name="IsManaged" type="String" static="true">enum field summary for IsManaged</field>
  /// <field name="IsValidForAdvancedFind" type="String" static="true">enum field summary for IsValidForAdvancedFind</field>	
  /// <field name="MetadataId" type="String" static="true">enum field summary for MetadataId</field>	
  /// <field name="ReferencedAttribute" type="String" static="true">enum field summary for ReferencedAttribute</field>	
  /// <field name="ReferencedEntity" type="String" static="true">enum field summary for ReferencedEntity</field>	
  /// <field name="ReferencingAttribute" type="String" static="true">enum field summary for ReferencingAttribute</field>	
  /// <field name="ReferencingEntity" type="String" static="true">enum field summary for ReferencingEntity</field>	
  /// <field name="RelationshipType" type="String" static="true">enum field summary for RelationshipType</field>	
  /// <field name="SchemaName" type="String" static="true">enum field summary for SchemaName</field>	
  /// <field name="SecurityTypes" type="String" static="true">enum field summary for SecurityTypes</field>
  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.EntityMetadataProperties = function () {
  /// <summary>SDK.Metadata.Query.EntityMetadataProperties enum summary</summary>
  /// <field name="ActivityTypeMask" type="String" static="true">enum field summary for ActivityTypeMask</field>	
  /// <field name="Attributes" type="String" static="true">enum field summary for Attributes</field>	
  /// <field name="AutoRouteToOwnerQueue" type="String" static="true">enum field summary for AutoRouteToOwnerQueue</field>	
  /// <field name="CanBeInManyToMany" type="String" static="true">enum field summary for CanBeInManyToMany</field>	
  /// <field name="CanBePrimaryEntityInRelationship" type="String" static="true">enum field summary for CanBePrimaryEntityInRelationship</field>	
  /// <field name="CanBeRelatedEntityInRelationship" type="String" static="true">enum field summary for CanBeRelatedEntityInRelationship</field>	
  /// <field name="CanCreateAttributes" type="String" static="true">enum field summary for CanCreateAttributes</field>	
  /// <field name="CanCreateCharts" type="String" static="true">enum field summary for CanCreateCharts</field>	
  /// <field name="CanCreateForms" type="String" static="true">enum field summary for CanCreateForms</field>	
  /// <field name="CanCreateViews" type="String" static="true">enum field summary for CanCreateViews</field>
  /// <field name="CanModifyAdditionalSettings" type="String" static="true">enum field summary for CanModifyAdditionalSettings</field>	
  /// <field name="CanTriggerWorkflow" type="String" static="true">enum field summary for CanTriggerWorkflow</field>	
  /// <field name="Description" type="String" static="true">enum field summary for Description</field>	
  /// <field name="DisplayCollectionName" type="String" static="true">enum field summary for DisplayCollectionName</field>	
  /// <field name="DisplayName" type="String" static="true">enum field summary for DisplayName</field>	
  /// <field name="IconLargeName" type="String" static="true">enum field summary for IconLargeName</field>	
  /// <field name="IconMediumName" type="String" static="true">enum field summary for IconMediumName</field>	
  /// <field name="IconSmallName" type="String" static="true">enum field summary for IconSmallName</field>	
  /// <field name="IsActivity" type="String" static="true">enum field summary for IsActivity</field>	
  /// <field name="IsActivityParty" type="String" static="true">enum field summary for IsActivityParty</field>
  /// <field name="IsAuditEnabled" type="String" static="true">enum field summary for IsAuditEnabled</field>	
  /// <field name="IsAvailableOffline" type="String" static="true">enum field summary for IsAvailableOffline</field>	
  /// <field name="IsChildEntity" type="String" static="true">enum field summary for IsChildEntity</field>	
  /// <field name="IsConnectionsEnabled" type="String" static="true">enum field summary for IsConnectionsEnabled</field>	
  /// <field name="IsCustomEntity" type="String" static="true">enum field summary for IsCustomEntity</field>	
  /// <field name="IsCustomizable" type="String" static="true">enum field summary for IsCustomizable</field>	
  /// <field name="IsDocumentManagementEnabled" type="String" static="true">enum field summary for IsDocumentManagementEnabled</field>	
  /// <field name="IsDuplicateDetectionEnabled" type="String" static="true">enum field summary for IsDuplicateDetectionEnabled</field>	
  /// <field name="IsEnabledForCharts" type="String" static="true">enum field summary for IsEnabledForCharts</field>	
  /// <field name="IsImportable" type="String" static="true">enum field summary for IsImportable</field>
  /// <field name="IsIntersect" type="String" static="true">enum field summary for IsIntersect</field>	
  /// <field name="IsMailMergeEnabled" type="String" static="true">enum field summary for IsMailMergeEnabled</field>	
  /// <field name="IsManaged" type="String" static="true">enum field summary for IsManaged</field>	
  /// <field name="IsMappable" type="String" static="true">enum field summary for IsMappable</field>	
  /// <field name="IsReadingPaneEnabled" type="String" static="true">enum field summary for IsReadingPaneEnabled</field>	
  /// <field name="IsRenameable" type="String" static="true">enum field summary for IsRenameable</field>	
  /// <field name="IsValidForAdvancedFind" type="String" static="true">enum field summary for IsValidForAdvancedFind</field>	
  /// <field name="IsValidForQueue" type="String" static="true">enum field summary for IsValidForQueue</field>	
  /// <field name="IsVisibleInMobile" type="String" static="true">enum field summary for IsVisibleInMobile</field>	
  /// <field name="LogicalName" type="String" static="true">enum field summary for LogicalName</field>
  /// <field name="ManyToManyRelationships" type="String" static="true">enum field summary for ManyToManyRelationships</field>	
  /// <field name="ManyToOneRelationships" type="String" static="true">enum field summary for ManyToOneRelationships</field>	
  /// <field name="MetadataId" type="String" static="true">enum field summary for MetadataId</field>	
  /// <field name="ObjectTypeCode" type="String" static="true">enum field summary for ObjectTypeCode</field>	
  /// <field name="OneToManyRelationships" type="String" static="true">enum field summary for OneToManyRelationships</field>	
  /// <field name="OwnershipType" type="String" static="true">enum field summary for OwnershipType</field>	
  /// <field name="PrimaryIdAttribute" type="String" static="true">enum field summary for PrimaryIdAttribute</field>	
  /// <field name="PrimaryNameAttribute" type="String" static="true">enum field summary for PrimaryNameAttribute</field>	
  /// <field name="Privileges" type="String" static="true">enum field summary for Privileges</field>	
  /// <field name="RecurrenceBaseEntityLogicalName" type="String" static="true">enum field summary for RecurrenceBaseEntityLogicalName</field>
  /// <field name="ReportViewName" type="String" static="true">enum field summary for ReportViewName</field>	
  /// <field name="SchemaName" type="String" static="true">enum field summary for SchemaName</field>
  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.AttributeMetadataProperties = function () {
  /// <summary>SDK.Metadata.Query.AttributeMetadataProperties enum summary</summary>
  /// <field name="AttributeOf" type="String" static="true">enum field summary for AttributeOf</field>	
  /// <field name="AttributeType" type="String" static="true">enum field summary for AttributeType</field>	
  /// <field name="CalculationOf" type="String" static="true">enum field summary for CalculationOf</field>	
  /// <field name="CanBeSecuredForCreate" type="String" static="true">enum field summary for CanBeSecuredForCreate</field>	
  /// <field name="CanBeSecuredForRead" type="String" static="true">enum field summary for CanBeSecuredForRead</field>	
  /// <field name="CanBeSecuredForUpdate" type="String" static="true">enum field summary for CanBeSecuredForUpdate</field>	
  /// <field name="CanModifyAdditionalSettings" type="String" static="true">enum field summary for CanModifyAdditionalSettings</field>	
  /// <field name="ColumnNumber" type="String" static="true">enum field summary for ColumnNumber</field>	
  /// <field name="DefaultFormValue" type="String" static="true">enum field summary for DefaultFormValue</field>	
  /// <field name="DefaultValue" type="String" static="true">enum field summary for DefaultValue</field>
  /// <field name="DeprecatedVersion" type="String" static="true">enum field summary for DeprecatedVersion</field>	
  /// <field name="Description" type="String" static="true">enum field summary for Description</field>	
  /// <field name="DisplayName" type="String" static="true">enum field summary for DisplayName</field>	
  /// <field name="EntityLogicalName" type="String" static="true">enum field summary for EntityLogicalName</field>	
  /// <field name="Format" type="String" static="true">enum field summary for Format</field>	
  /// <field name="ImeMode" type="String" static="true">enum field summary for ImeMode</field>	
  /// <field name="IsAuditEnabled" type="String" static="true">enum field summary for IsAuditEnabled</field>	
  /// <field name="IsCustomAttribute" type="String" static="true">enum field summary for IsCustomAttribute</field>	
  /// <field name="IsCustomizable" type="String" static="true">enum field summary for IsCustomizable</field>	
  /// <field name="IsManaged" type="String" static="true">enum field summary for IsManaged</field>
  /// <field name="IsPrimaryId" type="String" static="true">enum field summary for IsPrimaryId</field>	
  /// <field name="IsPrimaryName" type="String" static="true">enum field summary for IsPrimaryName</field>	
  /// <field name="IsRenameable" type="String" static="true">enum field summary for IsRenameable</field>	
  /// <field name="IsSecured" type="String" static="true">enum field summary for IsSecured</field>	
  /// <field name="IsValidForAdvancedFind" type="String" static="true">enum field summary for IsValidForAdvancedFind</field>	
  /// <field name="IsValidForCreate" type="String" static="true">enum field summary for IsValidForCreate</field>	
  /// <field name="IsValidForRead" type="String" static="true">enum field summary for IsValidForRead</field>	
  /// <field name="IsValidForUpdate" type="String" static="true">enum field summary for IsValidForUpdate</field>	
  /// <field name="LinkedAttributeId" type="String" static="true">enum field summary for LinkedAttributeId</field>	
  /// <field name="LogicalName" type="String" static="true">enum field summary for LogicalName</field>
  /// <field name="MaxLength" type="String" static="true">enum field summary for MaxLength</field>	
  /// <field name="MaxValue" type="String" static="true">enum field summary for MaxValue</field>	
  /// <field name="MetadataId" type="String" static="true">enum field summary for MetadataId</field>	
  /// <field name="MinValue" type="String" static="true">enum field summary for MinValue</field>	
  /// <field name="OptionSet" type="String" static="true">enum field summary for OptionSet</field>	
  /// <field name="Precision" type="String" static="true">enum field summary for Precision</field>	
  /// <field name="PrecisionSource" type="String" static="true">enum field summary for PrecisionSource</field>	
  /// <field name="RequiredLevel" type="String" static="true">enum field summary for RequiredLevel</field>	
  /// <field name="SchemaName" type="String" static="true">enum field summary for SchemaName</field>	
  /// <field name="Targets" type="String" static="true">enum field summary for Targets</field>
  /// <field name="YomiOf" type="String" static="true">enum field summary for YomiOf</field>	
  throw new Error("Constructor not implemented this is a static enum.");
 };

 this.RelationshipMetadataProperties = function () {
  /// <summary>SDK.Metadata.Query.RelationshipMetadataProperties enum summary</summary>
  /// <field name="AssociatedMenuConfiguration" type="String" static="true">enum field summary for AssociatedMenuConfiguration</field>	
  /// <field name="CascadeConfiguration" type="String" static="true">enum field summary for CascadeConfiguration</field>	
  /// <field name="Entity1AssociatedMenuConfiguration" type="String" static="true">enum field summary for Entity1AssociatedMenuConfiguration</field>	
  /// <field name="Entity1IntersectAttribute" type="String" static="true">enum field summary for Entity1IntersectAttribute</field>
  /// <field name="Entity1LogicalName" type="String" static="true">enum field summary for Entity1LogicalName</field>	
  /// <field name="Entity2AssociatedMenuConfiguration" type="String" static="true">enum field summary for Entity2AssociatedMenuConfiguration</field>	
  /// <field name="Entity2IntersectAttribute" type="String" static="true">enum field summary for Entity2IntersectAttribute</field>	
  /// <field name="Entity2LogicalName" type="String" static="true">enum field summary for Entity2LogicalName</field>
  /// <field name="IntersectEntityName" type="String" static="true">enum field summary for IntersectEntityName</field>	
  /// <field name="IsCustomizable" type="String" static="true">enum field summary for IsCustomizable</field>
  /// <field name="IsCustomRelationship" type="String" static="true">enum field summary for IsCustomRelationship</field>	
  /// <field name="IsManaged" type="String" static="true">enum field summary for IsManaged</field>	
  /// <field name="IsValidForAdvancedFind" type="String" static="true">enum field summary for IsValidForAdvancedFind</field>	
  /// <field name="MetadataId" type="String" static="true">enum field summary for MetadataId</field>
  /// <field name="ReferencedAttribute" type="String" static="true">enum field summary for ReferencedAttribute</field>	
  /// <field name="ReferencedEntity" type="String" static="true">enum field summary for ReferencedEntity</field>	
  /// <field name="ReferencingAttribute" type="String" static="true">enum field summary for ReferencingAttribute</field>	
  /// <field name="ReferencingEntity" type="String" static="true">enum field summary for ReferencingEntity</field>
  /// <field name="RelationshipType" type="String" static="true">enum field summary for RelationshipType</field>  
  /// <field name="SchemaName" type="String" static="true">enum field summary for SchemaName</field>	
  /// <field name="SecurityTypes" type="String" static="true">enum field summary for SecurityTypes</field>
  throw new Error("Constructor not implemented this is a static enum.");
 };
 //Enumerations part 1  END

 //RetrieveMetadataChanges START
 
		this.RetrieveMetadataChangesRequest = function (query, clientVersionStamp, deletedMetadataFilters)
	{
		///<summary>
		/// Initializes RetrieveMetadataChangesRequest to retrieve metadata using SDK.Metadata.RetrieveMetadataChanges
		///</summary>
		///<param name="query" type="Object">
		/// The SDK.Metadata.Query.EntityQueryExpression that defines the query
		///</param>
		///<param name="clientVersionStamp" optional="true"  type="String">
		/// The SDK.Metadata.Query.RetrieveMetadataChangesResponse.ServerVersionStamp value from a previous request.
		/// When included only the metadata changes since the previous request will be returned.
		///</param>
		///<param name="deletedMetadataFilters" optional="true"  type="Number">
		/// An SDK.Metadata.Query.DeletedMetadataFilters enumeration value.
		/// When included the deleted metadata changes will be limited to the types defined by the enumeration.
		///</param>
		if (!(this instanceof SDK.Metadata.Query.RetrieveMetadataChangesRequest))
		{
			return new SDK.Metadata.Query.RetrieveMetadataChangesRequest(query, clientVersionStamp, deletedMetadataFilters);
		}

		var _query = null;
		var _clientVersionStamp = null;
		var _deletedMetadataFilters = null;


		var queryIsNullMessage = "SDK.Metadata.Query.RetrieveMetadataChangesRequest Query is null.";
		var invalidQueryTypeMessage = "SDK.Metadata.Query.RetrieveMetadataChangesRequest.Query requires a SDK.Metadata.Query.EntityQueryExpression.";
		var invalidClientVersionStampMessage = "SDK.Metadata.Query.RetrieveMetadataChangesRequest ClientVersionStamp requires a string value.";
		var invalidDeletedMetadataFiltersMessage = "SDK.Metadata.Query.RetrieveMetadataChangesRequest DeletedMetadataFilters must be between 1 and 31.";

		function _setValidQuery(value)
		{
			if (value == null)
			{
				_query = value;
			}
			else
			{
				if (value instanceof SDK.Metadata.Query.EntityQueryExpression)
				{
					_query = value;
				}
				else
				{
					throw new Error(invalidQueryTypeMessage);
				}
			}

		}

		function _setValidClientVersionStamp(value)
		{
			if (value == null)
			{
				_clientVersionStamp = value;
			}
			else
			{
				if (typeof value == "string")
				{
					_clientVersionStamp = value;
				}
				else
				{
					throw new Error(invalidClientVersionStampMessage);
				}
			}
		}

		function _setValidDeletedMetadataFilters(value)
		{
			if (value == null)
			{
				_deletedMetadataFilters = value;
			}
			else
			{
				if (value >= 1 && value <= 31) //All
				{
					_deletedMetadataFilters = value;
				}
				else
				{
					throw new Error(invalidDeletedMetadataFiltersMessage)
				}
			}

		}

		//Set parameter values
		_setValidQuery(query)
		_setValidClientVersionStamp(clientVersionStamp)
		_setValidDeletedMetadataFilters(deletedMetadataFilters)



		this.getQuery = function () { 
  ///<summary>
		/// Gets SDK.Metadata.Query.EntityQueryExpression that defines the query.
		///</summary>
  return _query; };
		this.setQuery = function (value)
		{
  		///<summary>
		/// Sets SDK.Metadata.Query.EntityQueryExpression that defines the query.
		///</summary>
		///<param name="value" type="SDK.Metadata.Query.EntityQueryExpression">
		/// An SDK.Metadata.Query.EntityQueryExpression that defines the query
		///</param>
			_setValidQuery(value);
		};
		this.getClientVersionStamp = function () { 
  ///<summary>
		/// Gets the clientVersionStamp.
		///</summary>
  return _clientVersionStamp; };
		this.setClientVersionStamp = function (value)
		{
  ///<summary>
		/// Sets the clientVersionStamp.
		///</summary>
  		///<param name="value" type="SDK.Metadata.Query.EntityQueryExpression">
		/// An SDK.Metadata.Query.EntityQueryExpression that defines the query
		///</param>
			_setValidClientVersionStamp(value)
		};
		this.getDeletedMetadataFilters = function () { 
   ///<summary>
		/// Gets the DeletedMetadataFilters.
		///</summary>
  return _deletedMetadataFilters; };
		this.setDeletedMetadataFilters = function (value)
		{
    ///<summary>
		/// Sets the DeletedMetadataFilters.
		///</summary>
  		///<param name="value" type="Number">
		/// Use an SDK.Metadata.Query.DeletedMetadataFilters enumeration value.
		///</param>
			_setValidDeletedMetadataFilters(value);
		};
		this._toXml = function ()
		{
		///<summary>
  /// For internal use only.
  ///</summary>
			if (_query == null)
			{
				throw new Error(queryIsNullMessage)
			}

			var clientVersionStampXML = "";
			if (_clientVersionStamp != null)
			{
				clientVersionStampXML = ["<a:KeyValuePairOfstringanyType>",
            "<b:key>ClientVersionStamp</b:key>",
           "<b:value i:type=\"c:string\" xmlns:c=\"http://www.w3.org/2001/XMLSchema\">" + _clientVersionStamp + "</b:value>",
          "</a:KeyValuePairOfstringanyType>"].join("");
			}

			var deletedMetadataFiltersString = "Entity"; //Default

			if (_deletedMetadataFilters != null)
			{
				var deletedMetadataArray = [];
				if ((1 & _deletedMetadataFilters) == 1)
				{
					deletedMetadataArray.push("Entity");
				}
				if ((2 & _deletedMetadataFilters) == 2)
				{
					deletedMetadataArray.push("Attribute");
				}
				if ((4 & _deletedMetadataFilters) == 4)
				{
					deletedMetadataArray.push("Relationship");
				}
				if ((8 & _deletedMetadataFilters) == 8)
				{
					deletedMetadataArray.push("Label");
				}
				if ((16 & _deletedMetadataFilters) == 16)
				{
					deletedMetadataArray.push("OptionSet");
				}
				deletedMetadataFiltersString = deletedMetadataArray.join(" ");
			}


			var deletedMetadataFiltersXML = "";
			if (_clientVersionStamp != null && _deletedMetadataFilters != null)
			{
				deletedMetadataFiltersXML = ["<a:KeyValuePairOfstringanyType>",
            "<b:key>DeletedMetadataFilters</b:key>",
            "<b:value i:type=\"c:DeletedMetadataFilters\"",
                     " xmlns:c=\"http://schemas.microsoft.com/xrm/2011/Metadata/Query\">" + deletedMetadataFiltersString + "</b:value>",
          "</a:KeyValuePairOfstringanyType>"].join("");
			}


			return ["<request i:type=\"a:RetrieveMetadataChangesRequest\" xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\">",
        "<a:Parameters xmlns:b=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">",
          "<a:KeyValuePairOfstringanyType>",
            "<b:key>Query</b:key>",
												_query._toXml(),
          "</a:KeyValuePairOfstringanyType>",
            clientVersionStampXML,
          deletedMetadataFiltersXML,
        "</a:Parameters>",
        "<a:RequestId i:nil=\"true\" />",
        "<a:RequestName>RetrieveMetadataChanges</a:RequestName>",
      "</request>"].join("");
		};

	};

	this.RetrieveMetadataChangesResponse = function (entityMetadata, serverVersionStamp, deletedMetadata)
	{
		///<summary>
		/// The data returned from SDK.Metadata.Query.RetrieveMetadataChanges
		///</summary>

		if (!(this instanceof SDK.Metadata.Query.RetrieveMetadataChangesResponse))
		{
			return new SDK.Metadata.Query.RetrieveMetadataChangesResponse(entityMetadata, serverVersionStamp, deletedMetadata);
		}
		var _entityMetadata = null;
		var _serverVersionStamp = null;
		var _deletedMetadata = null;

		//set parameter values
		if (typeof entityMetadata != "undefined")
		{ _entityMetadata = entityMetadata; }
		if (typeof _serverVersionStamp != "undefined")
		{ _serverVersionStamp = serverVersionStamp; }
		if (typeof _deletedMetadata != "undefined")
		{ _deletedMetadata = deletedMetadata; }


		this.EntityMetadata = _entityMetadata;
		this.ServerVersionStamp = _serverVersionStamp;
		this.DeletedMetadata = _deletedMetadata;


	};

	this.RetrieveMetadataChanges = function (RetrieveMetadataChangesRequest, successCallBack, errorCallBack, passThroughObject)
	{
		///<summary>
		/// Sends an asynchronous request with success and error callback functions to process the results.
		///</summary>
		///<returns>SDK.Metadata.Query.RetrieveMetadataChangesResponse</returns>
		///<param name="RetrieveMetadataChangesRequest"  type="Object">
		/// A SDK.Metadata.Query.RetrieveMetadataChangesRequest that defines the metadata to retrieve.
		///</param>
		///<param name="successCallBack" type="Function">
		/// The function that will be passed through and be called by a successful response.
		/// This function must accept the SDK.Metadata.Query.RetrieveMetadataChangesResponse as a parameter.
		///</param>
		///<param name="errorCallBack" type="Function">
		/// The function that will be passed through and be called by a failed response.
		/// This function must accept an Error object as a parameter.
		///</param>
		///<param name="passThroughObject" type="Object">
		/// An Object that will be passed through to as the second parameter to the successCallBack.
		///</param>
		if (!(RetrieveMetadataChangesRequest instanceof SDK.Metadata.Query.RetrieveMetadataChangesRequest))
		{
			throw new Error("SDK.Metadata.Query.RetrieveMetadataChanges RetrieveMetadataChangesRequest parameter must be an SDK.Metadata.Query.RetrieveMetadataChangesRequest.");
		}
		if (typeof successCallBack != "function")
		{
			throw new Error("SDK.Metadata.Query.RetrieveMetadataChanges successCallBack parameter must be a function.")
		}
		if (typeof errorCallBack != "function")
		{
			throw new Error("SDK.Metadata.Query.RetrieveMetadataChanges errorCallBack parameter must be a function.")
		}

		var request = ["<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body>",
    "<Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">",
				RetrieveMetadataChangesRequest._toXml(),
				"</Execute></s:Body></s:Envelope>"].join("");


		var req = new XMLHttpRequest();
		req.open("POST", _getUrl() + "/XRMServices/2011/Organization.svc/web", true);
		try { req.responseType = 'msxml-document' } catch (e) { }
		req.setRequestHeader("Accept", "application/xml, text/xml, */*");
		req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
		req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
		req.onreadystatechange = function () { _processRetrievedMetadata(req, successCallBack, errorCallBack, passThroughObject) };
		req.send(request);


	};

		var _arrayElements = ["Attributes",
  "ManyToManyRelationships",
  "ManyToOneRelationships",
  "OneToManyRelationships",
  "Privileges",
  "LocalizedLabels",
  "Options",
  "Targets"];

		function _Context()
	{
		var errorMessage = "Context is not available.";
		if (typeof GetGlobalContext != "undefined")
		{ return GetGlobalContext(); }
		else
		{
			if (typeof Xrm != "undefined")
			{
				return Xrm.Page.context;
			}
			else
			{ return new Error(errorMessage); }
		}
	};

	function _getUrl()
		{
			if (typeof _Context().getServerUrl !== "undefined")
			{
				var url = _Context().getServerUrl();
				if (url.match(/\/$/))
				{
					url = url.substring(0, url.length - 1);
				}
			}

			if (typeof _Context().getClientUrl != "undefined")
		{
			url = _Context().getClientUrl();
		}
		return url;
	};

	function _evaluateEntityFilters(EntityFilters)
	{
		var entityFilterArray = [];
		if ((1 & EntityFilters) == 1)
		{
			entityFilterArray.push("Entity");
		}
		if ((2 & EntityFilters) == 2)
		{
			entityFilterArray.push("Attributes");
		}
		if ((4 & EntityFilters) == 4)
		{
			entityFilterArray.push("Privileges");
		}
		if ((8 & EntityFilters) == 8)
		{
			entityFilterArray.push("Relationships");
		}
		return entityFilterArray.join(" ");
	};

	function _processRetrievedMetadata(response, successCallBack, errorCallBack, passThroughObject)
	{
		if (response.readyState == 4 /* complete */)
		{
			response.onreadystatechange = null; //Addresses potential memory leak issue with IE
			if (response.status == 200)
			{
				//Success	
				var doc = response.responseXML;
				try { _setSelectionNamespaces(doc); } catch (e) { }

				var serverVersionStamp = _selectSingleNodeText(doc, "//a:Results/a:KeyValuePairOfstringanyType/b:value[../b:key/text()='ServerVersionStamp']");

				var entityMetadataCollection = [];

				var emNode = _selectSingleNode(doc, "//a:Results/a:KeyValuePairOfstringanyType/b:value[../b:key/text()='EntityMetadata']");
				for (var i = 0; i < emNode.childNodes.length; i++)
				{
					var a = _objectifyNode(emNode.childNodes[i]);
					a._type = "EntityMetadata";
					entityMetadataCollection.push(a);

				}


				var deletedMetadataCollection = {};

				var dmNode = _selectSingleNode(doc, "//a:Results/a:KeyValuePairOfstringanyType/b:value[../b:key/text()='DeletedMetadata']");
				if (dmNode != null)
				{
					for (var i = 0; i < dmNode.childNodes.length; i++)
					{
						var typeNode = dmNode.childNodes[i].firstChild;

						deletedMetadataCollection[_getNodeText(typeNode)] = [];
						for (var n = 0; n < typeNode.nextSibling.childNodes.length; n++)
						{

							var guidText = _getNodeText(typeNode.nextSibling.childNodes[n]);

							deletedMetadataCollection[_getNodeText(typeNode)].push(guidText);
						}
					}
				}

				//Convert into a SDK.Metadata.Query.RetrieveMetadataChangesResponse
				var rmcr = new SDK.Metadata.Query.RetrieveMetadataChangesResponse(entityMetadataCollection, serverVersionStamp, deletedMetadataCollection);


				successCallBack(rmcr, passThroughObject);

			}
			else
			{

				errorCallBack(_getError(response));

			}

		}

	};

	function _getError(resp)
	{
		///<summary>
		/// Private function that attempts to parse errors related to connectivity or WCF faults.
		///</summary>
		///<param name="resp" type="XMLHttpRequest">
		/// The XMLHttpRequest representing failed response.
		///</param>

		//Error descriptions come from http://support.microsoft.com/kb/193625
		if (resp.status == 12029)
		{ return new Error("The attempt to connect to the server failed."); }
		if (resp.status == 12007)
		{ return new Error("The server name could not be resolved."); }
		var faultXml = resp.responseXML;
		var errorMessage = "Unknown (unable to parse the fault)";
		if (typeof faultXml == "object")
		{

			var faultstring = null;
			var ErrorCode = null;

			var bodyNode = faultXml.firstChild.firstChild;

			//Retrieve the fault node
			for (var i = 0; i < bodyNode.childNodes.length; i++)
			{
				var node = bodyNode.childNodes[i];

				//NOTE: This comparison does not handle the case where the XML namespace changes
				if ("s:Fault" == node.nodeName)
				{
					for (var j = 0; j < node.childNodes.length; j++)
					{
						var testNode = node.childNodes[j];
						if ("faultstring" == testNode.nodeName)
						{
							faultstring = _getNodeText(testNode);
						}
						if ("detail" == testNode.nodeName)
						{
							for (var k = 0; k < testNode.childNodes.length; k++)
							{
								var orgServiceFault = testNode.childNodes[k];
								if ("OrganizationServiceFault" == orgServiceFault.nodeName)
								{
									for (var l = 0; l < orgServiceFault.childNodes.length; l++)
									{
										var ErrorCodeNode = orgServiceFault.childNodes[l];
										if ("ErrorCode" == ErrorCodeNode.nodeName)
										{
											ErrorCode = _getNodeText(ErrorCodeNode);
											break;
										}
									}
								}
							}

						}
					}
					break;
				}

			}
		}
		if (ErrorCode != null && faultstring != null)
		{
			errorMessage = "Error Code:" + ErrorCode + " Message: " + faultstring;
		}
		else
		{
			if (faultstring != null)
			{
				errorMessage = faultstring;
			}
		}
  if (errorMessage.indexOf("-2147204270") != -1) {
  return new Error("ExpiredVersionStamp: The clientVersionStamp value passed with the request is before the time specified in the Organization.ExpireSubscriptionsInDays value. Reinitialize your metadata cache using a null clientVersionStamp parameter.");
  }
		return new Error(errorMessage);
	};

	function _isMetadataArray(elementName)
	{
		for (var i = 0; i < _arrayElements.length; i++)
		{
			if (elementName == _arrayElements[i])
			{
				return true;
			}
		}
		return false;
	};

	function _objectifyNode(node)
	{
		//Check for null
		if (node.attributes != null && node.attributes.length == 1)
		{

			if (node.attributes.getNamedItem("i:nil") != null && node.attributes.getNamedItem("i:nil").nodeValue == "true")
			{
				return null;
			}
		}

		//Check if it is a value
		if ((node.firstChild != null) && (node.firstChild.nodeType == 3))
		{
			var nodeName = _getNodeName(node);

			switch (nodeName)
			{
				//Integer Values 
				case "ActivityTypeMask":
				case "ObjectTypeCode":
				case "ColumnNumber":
				case "DefaultFormValue":
				case "MaxValue":
				case "MinValue":
				case "MaxLength":
				case "Order":
				case "Precision":
				case "PrecisionSource":
				case "LanguageCode":
					return parseInt(node.firstChild.nodeValue, 10);
					// Boolean values
				case "AutoRouteToOwnerQueue":
				case "CanBeChanged":
				case "CanTriggerWorkflow":
				case "IsActivity":
				case "IsActivityParty":
				case "IsAvailableOffline":
				case "IsChildEntity":
				case "IsCustomEntity":
    case "IsCustomOptionSet":				
				case "IsDocumentManagementEnabled":
				case "IsEnabledForCharts":
    case "IsGlobal":
				case "IsImportable":
				case "IsIntersect":
				case "IsManaged":
				case "IsReadingPaneEnabled":
				case "IsValidForAdvancedFind":
				case "CanBeSecuredForCreate":
				case "CanBeSecuredForRead":
				case "CanBeSecuredForUpdate":
				case "IsCustomAttribute":
				case "IsManaged":
				case "IsPrimaryId":
				case "IsPrimaryName":
				case "IsSecured":
				case "IsValidForCreate":
				case "IsValidForRead":
				case "IsValidForUpdate":
				case "IsCustomRelationship":
				case "CanBeBasic":
				case "CanBeDeep":
				case "CanBeGlobal":
				case "CanBeLocal":
					return (node.firstChild.nodeValue == "true") ? true : false;
					//OptionMetadata.Value and BooleanManagedProperty.Value and AttributeRequiredLevelManagedProperty.Value
				case "Value":
					//BooleanManagedProperty.Value
					if ((node.firstChild.nodeValue == "true") || (node.firstChild.nodeValue == "false"))
					{
						return (node.firstChild.nodeValue == "true") ? true : false;
					}
					//AttributeRequiredLevelManagedProperty.Value
					if (
  (node.firstChild.nodeValue == "ApplicationRequired") ||
  (node.firstChild.nodeValue == "None") ||
  (node.firstChild.nodeValue == "Recommended") ||
  (node.firstChild.nodeValue == "SystemRequired")
  )
					{
						return node.firstChild.nodeValue;
					}
					else
					{
						//OptionMetadata.Value
						return parseInt(node.firstChild.nodeValue, 10);
					}
					break;
				//String values 
				default:
					return node.firstChild.nodeValue;
			}

		}

		//Check if it is a known array
		if (_isMetadataArray(_getNodeName(node)))
		{
			var arrayValue = [];

			for (var i = 0; i < node.childNodes.length; i++)
			{
				var objectTypeName;
				if ((node.childNodes[i].attributes != null) && (node.childNodes[i].attributes.getNamedItem("i:type") != null))
				{
					objectTypeName = node.childNodes[i].attributes.getNamedItem("i:type").nodeValue.split(":")[1];
				}
				else
				{

					objectTypeName = _getNodeName(node.childNodes[i]);
				}

				var b = _objectifyNode(node.childNodes[i]);
				b._type = objectTypeName;
				arrayValue.push(b);

			}

			return arrayValue;
		}

		//Null entity description labels are returned as <label/> - not using i:nil = true;
		if (node.childNodes.length == 0)
		{
			return null;
		}


		//Otherwise return an object
		var c = {};
		if (node.attributes.getNamedItem("i:type") != null)
		{
			c._type = node.attributes.getNamedItem("i:type").nodeValue.split(":")[1];
		}
		for (var i = 0; i < node.childNodes.length; i++)
		{
			if (node.childNodes[i].nodeType == 3)
			{
				c[_getNodeName(node.childNodes[i])] = node.childNodes[i].nodeValue;
			}
			else
			{
				c[_getNodeName(node.childNodes[i])] = _objectifyNode(node.childNodes[i]);
			}

		}
		return c;
	};

	function _selectNodes(node, XPathExpression)
	{
		if (typeof (node.selectNodes) != "undefined")
		{
			return node.selectNodes(XPathExpression);
		}
		else
		{
			var output = [];
			var XPathResults = node.evaluate(XPathExpression, node, _NSResolver, XPathResult.ANY_TYPE, null);
			var result = XPathResults.iterateNext();
			while (result)
			{
				output.push(result);
				result = XPathResults.iterateNext();
			}
			return output;
		}
	};

	function _selectSingleNode(node, xpathExpr)
	{
		if (typeof (node.selectSingleNode) != "undefined")
		{
			return node.selectSingleNode(xpathExpr);
		}
		else
		{
			var xpe = new XPathEvaluator();
			var xPathNode = xpe.evaluate(xpathExpr, node, _NSResolver, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
			return (xPathNode != null) ? xPathNode.singleNodeValue : null;
		}
	};

	function _selectSingleNodeText(node, xpathExpr)
	{
		var x = _selectSingleNode(node, xpathExpr);
		if (_isNodeNull(x))
		{ return null; }
		if (typeof (x.text) != "undefined")
		{
			return x.text;
		}
		else
		{
			return x.textContent;
		}
	};

	function _getNodeText(node)
	{
		if (typeof (node.text) != "undefined")
		{
			return node.text;
		}
		else
		{
			return node.textContent;
		}
	};

	function _isNodeNull(node)
	{
		if (node == null)
		{ return true; }
		if ((node.attributes.getNamedItem("i:nil") != null) && (node.attributes.getNamedItem("i:nil").value == "true"))
		{ return true; }
		return false;
	};

	function _getNodeName(node)
	{
		if (typeof (node.baseName) != "undefined")
		{
			return node.baseName;
		}
		else
		{
			return node.localName;
		}
	};

	function _setSelectionNamespaces(doc)
	{
		var namespaces = [
 "xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'",
 "xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts'",
 "xmlns:i='http://www.w3.org/2001/XMLSchema-instance'",
 "xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'",
 "xmlns:c='http://schemas.microsoft.com/xrm/2011/Metadata'"
 ];
		doc.setProperty("SelectionNamespaces", namespaces.join(" "));

	}

	function _NSResolver(prefix)
	{
		var ns = {
			"s": "http://schemas.xmlsoap.org/soap/envelope/",
			"a": "http://schemas.microsoft.com/xrm/2011/Contracts",
			"i": "http://www.w3.org/2001/XMLSchema-instance",
			"b": "http://schemas.datacontract.org/2004/07/System.Collections.Generic",
			"c": "http://schemas.microsoft.com/xrm/2011/Metadata"
		};
		return ns[prefix] || null;
	};

	function _xmlEncode(strInput)
	{
		var c;
		var XmlEncode = '';
		if (strInput == null)
		{
			return null;
		}
		if (strInput == '')
		{
			return '';
		}
		for (var cnt = 0; cnt < strInput.length; cnt++)
		{
			c = strInput.charCodeAt(cnt);
			if (((c > 96) && (c < 123)) ||
			((c > 64) && (c < 91)) ||
			(c == 32) ||
			((c > 47) && (c < 58)) ||
			(c == 46) ||
			(c == 44) ||
			(c == 45) ||
			(c == 95))
			{
				XmlEncode = XmlEncode + String.fromCharCode(c);
			}
			else
			{
				XmlEncode = XmlEncode + '&#' + c + ';';
			}
		}
		return XmlEncode;
	};
 //RetrieveMetadataChanges END

}).call(SDK.Metadata.Query);


//Enumerations part 2 START
//These enumerations are defined so that they provide a development experience
// in Visual Studio that is consistent with strongly typed languages like C#.
//SDK.Metadata.Query.LogicalOperator
SDK.Metadata.Query.LogicalOperator.prototype = { And: "And", Or: "Or" };
SDK.Metadata.Query.LogicalOperator.And = "And";
SDK.Metadata.Query.LogicalOperator.Or = "Or";
SDK.Metadata.Query.LogicalOperator.__enum = true;
SDK.Metadata.Query.LogicalOperator.__flags = true;

//SDK.Metadata.Query.DeletedMetadataFilters
SDK.Metadata.Query.DeletedMetadataFilters.prototype = {
 Default: 1,
 Entity: 1,
 Attribute: 2,
 Relationship: 4,
 Label: 8,
 OptionSet: 16,
 All: 31
};
SDK.Metadata.Query.DeletedMetadataFilters.Default = 1;
SDK.Metadata.Query.DeletedMetadataFilters.Entity = 1;
SDK.Metadata.Query.DeletedMetadataFilters.Attribute = 2;
SDK.Metadata.Query.DeletedMetadataFilters.Relationship = 4;
SDK.Metadata.Query.DeletedMetadataFilters.Label = 8;
SDK.Metadata.Query.DeletedMetadataFilters.OptionSet = 16;
SDK.Metadata.Query.DeletedMetadataFilters.All = 31;
SDK.Metadata.Query.DeletedMetadataFilters.__enum = true;
SDK.Metadata.Query.DeletedMetadataFilters.__flags = true;

//SDK.Metadata.Query.MetadataConditionOperator
SDK.Metadata.Query.MetadataConditionOperator.prototype = {
 Equals: "Equals",
 NotEquals: "NotEquals",
 In: "In",
 NotIn: "NotIn",
 GreaterThan: "GreaterThan",
 LessThan: "LessThan"
};
SDK.Metadata.Query.MetadataConditionOperator.Equals = "Equals";
SDK.Metadata.Query.MetadataConditionOperator.NotEquals = "NotEquals";
SDK.Metadata.Query.MetadataConditionOperator.In = "In";
SDK.Metadata.Query.MetadataConditionOperator.NotIn = "NotIn";
SDK.Metadata.Query.MetadataConditionOperator.GreaterThan = "GreaterThan";
SDK.Metadata.Query.MetadataConditionOperator.LessThan = "LessThan";
SDK.Metadata.Query.MetadataConditionOperator.__enum = true;
SDK.Metadata.Query.MetadataConditionOperator.__flags = true;

//SDK.Metadata.Query.SearchableEntityMetadataProperties
SDK.Metadata.Query.SearchableEntityMetadataProperties.prototype = {
 ActivityTypeMask: "ActivityTypeMask",
 AutoRouteToOwnerQueue: "AutoRouteToOwnerQueue",
 CanBeInManyToMany: "CanBeInManyToMany",
 CanBePrimaryEntityInRelationship: "CanBePrimaryEntityInRelationship",
 CanBeRelatedEntityInRelationship: "CanBeRelatedEntityInRelationship",
 CanCreateAttributes: "CanCreateAttributes",
 CanCreateCharts: "CanCreateCharts",
 CanCreateForms: "CanCreateForms",
 CanCreateViews: "CanCreateViews",
 CanModifyAdditionalSettings: "CanModifyAdditionalSettings",
 CanTriggerWorkflow: "CanTriggerWorkflow",
 IconLargeName: "IconLargeName",
 IconMediumName: "IconMediumName",
 IconSmallName: "IconSmallName",
 IsActivity: "IsActivity",
 IsActivityParty: "IsActivityParty",
 IsAuditEnabled: "IsAuditEnabled",
 IsAvailableOffline: "IsAvailableOffline",
 IsChildEntity: "IsChildEntity",
 IsConnectionsEnabled: "IsConnectionsEnabled",
 IsCustomEntity: "IsCustomEntity",
 IsCustomizable: "IsCustomizable",
 IsDocumentManagementEnabled: "IsDocumentManagementEnabled",
 IsDuplicateDetectionEnabled: "IsDuplicateDetectionEnabled",
 IsEnabledForCharts: "IsEnabledForCharts",
 IsImportable: "IsImportable",
 IsIntersect: "IsIntersect",
 IsMailMergeEnabled: "IsMailMergeEnabled",
 IsManaged: "IsManaged",
 IsMappable: "IsMappable",
 IsReadingPaneEnabled: "IsReadingPaneEnabled",
 IsRenameable: "IsRenameable",
 IsValidForAdvancedFind: "IsValidForAdvancedFind",
 IsValidForQueue: "IsValidForQueue",
 IsVisibleInMobile: "IsVisibleInMobile",
 LogicalName: "LogicalName",
 MetadataId: "MetadataId",
 ObjectTypeCode: "ObjectTypeCode",
 OwnershipType: "OwnershipType",
 PrimaryIdAttribute: "PrimaryIdAttribute",
 PrimaryNameAttribute: "PrimaryNameAttribute",
 RecurrenceBaseEntityLogicalName: "RecurrenceBaseEntityLogicalName",
 ReportViewName: "ReportViewName",
 SchemaName: "SchemaName"
};
SDK.Metadata.Query.SearchableEntityMetadataProperties.ActivityTypeMask = "ActivityTypeMask";
SDK.Metadata.Query.SearchableEntityMetadataProperties.AutoRouteToOwnerQueue = "AutoRouteToOwnerQueue";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanBeInManyToMany = "CanBeInManyToMany";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanBePrimaryEntityInRelationship = "CanBePrimaryEntityInRelationship";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanBeRelatedEntityInRelationship = "CanBeRelatedEntityInRelationship";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanCreateAttributes = "CanCreateAttributes";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanCreateCharts = "CanCreateCharts";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanCreateForms = "CanCreateForms";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanCreateViews = "CanCreateViews";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanModifyAdditionalSettings = "CanModifyAdditionalSettings";
SDK.Metadata.Query.SearchableEntityMetadataProperties.CanTriggerWorkflow = "CanTriggerWorkflow";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IconLargeName = "IconLargeName";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IconMediumName = "IconMediumName";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IconSmallName = "IconSmallName";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsActivity = "IsActivity";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsActivityParty = "IsActivityParty";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsAuditEnabled = "IsAuditEnabled";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsAvailableOffline = "IsAvailableOffline";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsChildEntity = "IsChildEntity";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsConnectionsEnabled = "IsConnectionsEnabled";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsCustomEntity = "IsCustomEntity";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsCustomizable = "IsCustomizable";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsDocumentManagementEnabled = "IsDocumentManagementEnabled";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsDuplicateDetectionEnabled = "IsDuplicateDetectionEnabled";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsEnabledForCharts = "IsEnabledForCharts";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsImportable = "IsImportable";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsIntersect = "IsIntersect";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsMailMergeEnabled = "IsMailMergeEnabled";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsManaged = "IsManaged";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsMappable = "IsMappable";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsReadingPaneEnabled = "IsReadingPaneEnabled";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsRenameable = "IsRenameable";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsValidForAdvancedFind = "IsValidForAdvancedFind";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsValidForQueue = "IsValidForQueue";
SDK.Metadata.Query.SearchableEntityMetadataProperties.IsVisibleInMobile = "IsVisibleInMobile";
SDK.Metadata.Query.SearchableEntityMetadataProperties.LogicalName = "LogicalName";
SDK.Metadata.Query.SearchableEntityMetadataProperties.MetadataId = "MetadataId";
SDK.Metadata.Query.SearchableEntityMetadataProperties.ObjectTypeCode = "ObjectTypeCode";
SDK.Metadata.Query.SearchableEntityMetadataProperties.OwnershipType = "OwnershipType";
SDK.Metadata.Query.SearchableEntityMetadataProperties.PrimaryIdAttribute = "PrimaryIdAttribute";
SDK.Metadata.Query.SearchableEntityMetadataProperties.PrimaryNameAttribute = "PrimaryNameAttribute";
SDK.Metadata.Query.SearchableEntityMetadataProperties.RecurrenceBaseEntityLogicalName = "RecurrenceBaseEntityLogicalName";
SDK.Metadata.Query.SearchableEntityMetadataProperties.ReportViewName = "ReportViewName";
SDK.Metadata.Query.SearchableEntityMetadataProperties.SchemaName = "SchemaName";
SDK.Metadata.Query.SearchableEntityMetadataProperties.__enum = true;
SDK.Metadata.Query.SearchableEntityMetadataProperties.__flags = true;

//SDK.Metadata.Query.SearchableAttributeMetadataProperties
SDK.Metadata.Query.SearchableAttributeMetadataProperties.prototype = {
 AttributeOf: "AttributeOf",
 AttributeType: "AttributeType",
 CalculationOf: "CalculationOf",
 CanBeSecuredForCreate: "CanBeSecuredForCreate",
 CanBeSecuredForRead: "CanBeSecuredForRead",
 CanBeSecuredForUpdate: "CanBeSecuredForUpdate",
 CanModifyAdditionalSettings: "CanModifyAdditionalSettings",
 ColumnNumber: "ColumnNumber",
 DefaultFormValue: "DefaultFormValue",
 DefaultValue: "DefaultValue",
 DeprecatedVersion: "DeprecatedVersion",
 EntityLogicalName: "EntityLogicalName",
 Format: "Format",
 ImeMode: "ImeMode",
 IsAuditEnabled: "IsAuditEnabled",
 IsCustomAttribute: "IsCustomAttribute",
 IsCustomizable: "IsCustomizable",
 IsManaged: "IsManaged",
 IsPrimaryId: "IsPrimaryId",
 IsPrimaryName: "IsPrimaryName",
 IsRenameable: "IsRenameable",
 IsSecured: "IsSecured",
 IsValidForAdvancedFind: "IsValidForAdvancedFind",
 IsValidForCreate: "IsValidForCreate",
 IsValidForRead: "IsValidForRead",
 IsValidForUpdate: "IsValidForUpdate",
 LinkedAttributeId: "LinkedAttributeId",
 LogicalName: "LogicalName",
 MaxLength: "MaxLength",
 MaxValue: "MaxValue",
 MetadataId: "MetadataId",
 MinValue: "MinValue",
 Precision: "Precision",
 PrecisionSource: "PrecisionSource",
 RequiredLevel: "RequiredLevel",
 SchemaName: "SchemaName",
 YomiOf: "YomiOf"
};
SDK.Metadata.Query.SearchableAttributeMetadataProperties.AttributeOf = "AttributeOf";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.AttributeType = "AttributeType";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.CalculationOf = "CalculationOf";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.CanBeSecuredForCreate = "CanBeSecuredForCreate";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.CanBeSecuredForRead = "CanBeSecuredForRead";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.CanBeSecuredForUpdate = "CanBeSecuredForUpdate";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.CanModifyAdditionalSettings = "CanModifyAdditionalSettings";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.ColumnNumber = "ColumnNumber";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.DefaultFormValue = "DefaultFormValue";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.DefaultValue = "DefaultValue";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.DeprecatedVersion = "DeprecatedVersion";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.EntityLogicalName = "EntityLogicalName";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.Format = "Format";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.ImeMode = "ImeMode";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsAuditEnabled = "IsAuditEnabled";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsCustomAttribute = "IsCustomAttribute";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsCustomizable = "IsCustomizable";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsManaged = "IsManaged";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsPrimaryId = "IsPrimaryId";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsPrimaryName = "IsPrimaryName";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsRenameable = "IsRenameable";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsSecured = "IsSecured";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsValidForAdvancedFind = "IsValidForAdvancedFind";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsValidForCreate = "IsValidForCreate";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsValidForRead = "IsValidForRead";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.IsValidForUpdate = "IsValidForUpdate";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.LinkedAttributeId = "LinkedAttributeId";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.LogicalName = "LogicalName";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.MaxLength = "MaxLength";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.MaxValue = "MaxValue";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.MetadataId = "MetadataId";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.MinValue = "MinValue";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.Precision = "Precision";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.PrecisionSource = "PrecisionSource";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.RequiredLevel = "RequiredLevel";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.SchemaName = "SchemaName";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.YomiOf = "YomiOf";
SDK.Metadata.Query.SearchableAttributeMetadataProperties.__enum = true;
SDK.Metadata.Query.SearchableAttributeMetadataProperties.__flags = true;

//SDK.Metadata.Query.SearchableRelationshipMetadataProperties
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.prototype = {
 Entity1IntersectAttribute: "Entity1IntersectAttribute",
 Entity1LogicalName: "Entity1LogicalName",
 Entity2IntersectAttribute: "Entity2IntersectAttribute",
 Entity2LogicalName: "Entity2LogicalName",
 IntersectEntityName: "IntersectEntityName",
 IsCustomizable: "IsCustomizable",
 IsCustomRelationship: "IsCustomRelationship",
 IsManaged: "IsManaged",
 IsValidForAdvancedFind: "IsValidForAdvancedFind",
 MetadataId: "MetadataId",
 ReferencedAttribute: "ReferencedAttribute",
 ReferencedEntity: "ReferencedEntity",
 ReferencingAttribute: "ReferencingAttribute",
 ReferencingEntity: "ReferencingEntity",
 RelationshipType: "RelationshipType",
 SchemaName: "SchemaName",
 SecurityTypes: "SecurityTypes"
};
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.Entity1IntersectAttribute = "Entity1IntersectAttribute";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.Entity1LogicalName = "Entity1LogicalName";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.Entity2IntersectAttribute = "Entity2IntersectAttribute";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.Entity2LogicalName = "Entity2LogicalName";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.IntersectEntityName = "IntersectEntityName";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.IsCustomizable = "IsCustomizable";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.IsCustomRelationship = "IsCustomRelationship";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.IsManaged = "IsManaged";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.IsValidForAdvancedFind = "IsValidForAdvancedFind";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.MetadataId = "MetadataId";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.ReferencedAttribute = "ReferencedAttribute";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.ReferencedEntity = "ReferencedEntity";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.ReferencingAttribute = "ReferencingAttribute";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.ReferencingEntity = "ReferencingEntity";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.RelationshipType = "RelationshipType";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.SchemaName = "SchemaName";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.SecurityTypes = "SecurityTypes";
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.__enum = true;
SDK.Metadata.Query.SearchableRelationshipMetadataProperties.__flags = true;

//SDK.Metadata.Query.EntityMetadataProperties
SDK.Metadata.Query.EntityMetadataProperties.prototype = {
 ActivityTypeMask: "ActivityTypeMask",
 Attributes: "Attributes",
 AutoRouteToOwnerQueue: "AutoRouteToOwnerQueue",
 CanBeInManyToMany: "CanBeInManyToMany",
 CanBePrimaryEntityInRelationship: "CanBePrimaryEntityInRelationship",
 CanBeRelatedEntityInRelationship: "CanBeRelatedEntityInRelationship",
 CanCreateAttributes: "CanCreateAttributes",
 CanCreateCharts: "CanCreateCharts",
 CanCreateForms: "CanCreateForms",
 CanCreateViews: "CanCreateViews",
 CanModifyAdditionalSettings: "CanModifyAdditionalSettings",
 CanTriggerWorkflow: "CanTriggerWorkflow",
 Description: "Description",
 DisplayCollectionName: "DisplayCollectionName",
 DisplayName: "DisplayName",
 IconLargeName: "IconLargeName",
 IconMediumName: "IconMediumName",
 IconSmallName: "IconSmallName",
 IsActivity: "IsActivity",
 IsActivityParty: "IsActivityParty",
 IsAuditEnabled: "IsAuditEnabled",
 IsAvailableOffline: "IsAvailableOffline",
 IsChildEntity: "IsChildEntity",
 IsConnectionsEnabled: "IsConnectionsEnabled",
 IsCustomEntity: "IsCustomEntity",
 IsCustomizable: "IsCustomizable",
 IsDocumentManagementEnabled: "IsDocumentManagementEnabled",
 IsDuplicateDetectionEnabled: "IsDuplicateDetectionEnabled",
 IsEnabledForCharts: "IsEnabledForCharts",
 IsImportable: "IsImportable",
 IsIntersect: "IsIntersect",
 IsMailMergeEnabled: "IsMailMergeEnabled",
 IsManaged: "IsManaged",
 IsMappable: "IsMappable",
 IsReadingPaneEnabled: "IsReadingPaneEnabled",
 IsRenameable: "IsRenameable",
 IsValidForAdvancedFind: "IsValidForAdvancedFind",
 IsValidForQueue: "IsValidForQueue",
 IsVisibleInMobile: "IsVisibleInMobile",
 LogicalName: "LogicalName",
 ManyToManyRelationships: "ManyToManyRelationships",
 ManyToOneRelationships: "ManyToOneRelationships",
 MetadataId: "MetadataId",
 ObjectTypeCode: "ObjectTypeCode",
 OneToManyRelationships: "OneToManyRelationships",
 OwnershipType: "OwnershipType",
 PrimaryIdAttribute: "PrimaryIdAttribute",
 PrimaryNameAttribute: "PrimaryNameAttribute",
 Privileges: "Privileges",
 RecurrenceBaseEntityLogicalName: "RecurrenceBaseEntityLogicalName",
 ReportViewName: "ReportViewName",
 SchemaName: "SchemaName"
};
SDK.Metadata.Query.EntityMetadataProperties.ActivityTypeMask = "ActivityTypeMask";
SDK.Metadata.Query.EntityMetadataProperties.Attributes = "Attributes";
SDK.Metadata.Query.EntityMetadataProperties.AutoRouteToOwnerQueue = "AutoRouteToOwnerQueue";
SDK.Metadata.Query.EntityMetadataProperties.CanBeInManyToMany = "CanBeInManyToMany";
SDK.Metadata.Query.EntityMetadataProperties.CanBePrimaryEntityInRelationship = "CanBePrimaryEntityInRelationship";
SDK.Metadata.Query.EntityMetadataProperties.CanBeRelatedEntityInRelationship = "CanBeRelatedEntityInRelationship";
SDK.Metadata.Query.EntityMetadataProperties.CanCreateAttributes = "CanCreateAttributes";
SDK.Metadata.Query.EntityMetadataProperties.CanCreateCharts = "CanCreateCharts";
SDK.Metadata.Query.EntityMetadataProperties.CanCreateForms = "CanCreateForms";
SDK.Metadata.Query.EntityMetadataProperties.CanCreateViews = "CanCreateViews";
SDK.Metadata.Query.EntityMetadataProperties.CanModifyAdditionalSettings = "CanModifyAdditionalSettings";
SDK.Metadata.Query.EntityMetadataProperties.CanTriggerWorkflow = "CanTriggerWorkflow";
SDK.Metadata.Query.EntityMetadataProperties.Description = "Description";
SDK.Metadata.Query.EntityMetadataProperties.DisplayCollectionName = "DisplayCollectionName";
SDK.Metadata.Query.EntityMetadataProperties.DisplayName = "DisplayName";
SDK.Metadata.Query.EntityMetadataProperties.IconLargeName = "IconLargeName";
SDK.Metadata.Query.EntityMetadataProperties.IconMediumName = "IconMediumName";
SDK.Metadata.Query.EntityMetadataProperties.IconSmallName = "IconSmallName";
SDK.Metadata.Query.EntityMetadataProperties.IsActivity = "IsActivity";
SDK.Metadata.Query.EntityMetadataProperties.IsActivityParty = "IsActivityParty";
SDK.Metadata.Query.EntityMetadataProperties.IsAuditEnabled = "IsAuditEnabled";
SDK.Metadata.Query.EntityMetadataProperties.IsAvailableOffline = "IsAvailableOffline";
SDK.Metadata.Query.EntityMetadataProperties.IsChildEntity = "IsChildEntity";
SDK.Metadata.Query.EntityMetadataProperties.IsConnectionsEnabled = "IsConnectionsEnabled";
SDK.Metadata.Query.EntityMetadataProperties.IsCustomEntity = "IsCustomEntity";
SDK.Metadata.Query.EntityMetadataProperties.IsCustomizable = "IsCustomizable";
SDK.Metadata.Query.EntityMetadataProperties.IsDocumentManagementEnabled = "IsDocumentManagementEnabled";
SDK.Metadata.Query.EntityMetadataProperties.IsDuplicateDetectionEnabled = "IsDuplicateDetectionEnabled";
SDK.Metadata.Query.EntityMetadataProperties.IsEnabledForCharts = "IsEnabledForCharts";
SDK.Metadata.Query.EntityMetadataProperties.IsImportable = "IsImportable";
SDK.Metadata.Query.EntityMetadataProperties.IsIntersect = "IsIntersect";
SDK.Metadata.Query.EntityMetadataProperties.IsMailMergeEnabled = "IsMailMergeEnabled";
SDK.Metadata.Query.EntityMetadataProperties.IsManaged = "IsManaged";
SDK.Metadata.Query.EntityMetadataProperties.IsMappable = "IsMappable";
SDK.Metadata.Query.EntityMetadataProperties.IsReadingPaneEnabled = "IsReadingPaneEnabled";
SDK.Metadata.Query.EntityMetadataProperties.IsRenameable = "IsRenameable";
SDK.Metadata.Query.EntityMetadataProperties.IsValidForAdvancedFind = "IsValidForAdvancedFind";
SDK.Metadata.Query.EntityMetadataProperties.IsValidForQueue = "IsValidForQueue";
SDK.Metadata.Query.EntityMetadataProperties.IsVisibleInMobile = "IsVisibleInMobile";
SDK.Metadata.Query.EntityMetadataProperties.LogicalName = "LogicalName";
SDK.Metadata.Query.EntityMetadataProperties.ManyToManyRelationships = "ManyToManyRelationships";
SDK.Metadata.Query.EntityMetadataProperties.ManyToOneRelationships = "ManyToOneRelationships";
SDK.Metadata.Query.EntityMetadataProperties.MetadataId = "MetadataId";
SDK.Metadata.Query.EntityMetadataProperties.ObjectTypeCode = "ObjectTypeCode";
SDK.Metadata.Query.EntityMetadataProperties.OneToManyRelationships = "OneToManyRelationships";
SDK.Metadata.Query.EntityMetadataProperties.OwnershipType = "OwnershipType";
SDK.Metadata.Query.EntityMetadataProperties.PrimaryIdAttribute = "PrimaryIdAttribute";
SDK.Metadata.Query.EntityMetadataProperties.PrimaryNameAttribute = "PrimaryNameAttribute";
SDK.Metadata.Query.EntityMetadataProperties.Privileges = "Privileges";
SDK.Metadata.Query.EntityMetadataProperties.RecurrenceBaseEntityLogicalName = "RecurrenceBaseEntityLogicalName";
SDK.Metadata.Query.EntityMetadataProperties.ReportViewName = "ReportViewName";
SDK.Metadata.Query.EntityMetadataProperties.SchemaName = "SchemaName";
SDK.Metadata.Query.EntityMetadataProperties.__enum = true;
SDK.Metadata.Query.EntityMetadataProperties.__flags = true;

//SDK.Metadata.Query.AttributeMetadataProperties
SDK.Metadata.Query.AttributeMetadataProperties.prototype = {
 AttributeOf: "AttributeOf",
 AttributeType: "AttributeType",
 CalculationOf: "CalculationOf",
 CanBeSecuredForCreate: "CanBeSecuredForCreate",
 CanBeSecuredForRead: "CanBeSecuredForRead",
 CanBeSecuredForUpdate: "CanBeSecuredForUpdate",
 CanModifyAdditionalSettings: "CanModifyAdditionalSettings",
 ColumnNumber: "ColumnNumber",
 DefaultFormValue: "DefaultFormValue",
 DefaultValue: "DefaultValue",
 DeprecatedVersion: "DeprecatedVersion",
 Description: "Description",
 DisplayName: "DisplayName",
 EntityLogicalName: "EntityLogicalName",
 Format: "Format",
 ImeMode: "ImeMode",
 IsAuditEnabled: "IsAuditEnabled",
 IsCustomAttribute: "IsCustomAttribute",
 IsCustomizable: "IsCustomizable",
 IsManaged: "IsManaged",
 IsPrimaryId: "IsPrimaryId",
 IsPrimaryName: "IsPrimaryName",
 IsRenameable: "IsRenameable",
 IsSecured: "IsSecured",
 IsValidForAdvancedFind: "IsValidForAdvancedFind",
 IsValidForCreate: "IsValidForCreate",
 IsValidForRead: "IsValidForRead",
 IsValidForUpdate: "IsValidForUpdate",
 LinkedAttributeId: "LinkedAttributeId",
 LogicalName: "LogicalName",
 MaxLength: "MaxLength",
 MaxValue: "MaxValue",
 MetadataId: "MetadataId",
 MinValue: "MinValue",
 OptionSet: "OptionSet",
 Precision: "Precision",
 PrecisionSource: "PrecisionSource",
 RequiredLevel: "RequiredLevel",
 SchemaName: "SchemaName",
 Targets: "Targets",
 YomiOf: "YomiOf"
};
SDK.Metadata.Query.AttributeMetadataProperties.AttributeOf = "AttributeOf";
SDK.Metadata.Query.AttributeMetadataProperties.AttributeType = "AttributeType";
SDK.Metadata.Query.AttributeMetadataProperties.CalculationOf = "CalculationOf";
SDK.Metadata.Query.AttributeMetadataProperties.CanBeSecuredForCreate = "CanBeSecuredForCreate";
SDK.Metadata.Query.AttributeMetadataProperties.CanBeSecuredForRead = "CanBeSecuredForRead";
SDK.Metadata.Query.AttributeMetadataProperties.CanBeSecuredForUpdate = "CanBeSecuredForUpdate";
SDK.Metadata.Query.AttributeMetadataProperties.CanModifyAdditionalSettings = "CanModifyAdditionalSettings";
SDK.Metadata.Query.AttributeMetadataProperties.ColumnNumber = "ColumnNumber";
SDK.Metadata.Query.AttributeMetadataProperties.DefaultFormValue = "DefaultFormValue";
SDK.Metadata.Query.AttributeMetadataProperties.DefaultValue = "DefaultValue";
SDK.Metadata.Query.AttributeMetadataProperties.DeprecatedVersion = "DeprecatedVersion";
SDK.Metadata.Query.AttributeMetadataProperties.Description = "Description";
SDK.Metadata.Query.AttributeMetadataProperties.DisplayName = "DisplayName";
SDK.Metadata.Query.AttributeMetadataProperties.EntityLogicalName = "EntityLogicalName";
SDK.Metadata.Query.AttributeMetadataProperties.Format = "Format";
SDK.Metadata.Query.AttributeMetadataProperties.ImeMode = "ImeMode";
SDK.Metadata.Query.AttributeMetadataProperties.IsAuditEnabled = "IsAuditEnabled";
SDK.Metadata.Query.AttributeMetadataProperties.IsCustomAttribute = "IsCustomAttribute";
SDK.Metadata.Query.AttributeMetadataProperties.IsCustomizable = "IsCustomizable";
SDK.Metadata.Query.AttributeMetadataProperties.IsManaged = "IsManaged";
SDK.Metadata.Query.AttributeMetadataProperties.IsPrimaryId = "IsPrimaryId";
SDK.Metadata.Query.AttributeMetadataProperties.IsPrimaryName = "IsPrimaryName";
SDK.Metadata.Query.AttributeMetadataProperties.IsRenameable = "IsRenameable";
SDK.Metadata.Query.AttributeMetadataProperties.IsSecured = "IsSecured";
SDK.Metadata.Query.AttributeMetadataProperties.IsValidForAdvancedFind = "IsValidForAdvancedFind";
SDK.Metadata.Query.AttributeMetadataProperties.IsValidForCreate = "IsValidForCreate";
SDK.Metadata.Query.AttributeMetadataProperties.IsValidForRead = "IsValidForRead";
SDK.Metadata.Query.AttributeMetadataProperties.IsValidForUpdate = "IsValidForUpdate";
SDK.Metadata.Query.AttributeMetadataProperties.LinkedAttributeId = "LinkedAttributeId";
SDK.Metadata.Query.AttributeMetadataProperties.LogicalName = "LogicalName";
SDK.Metadata.Query.AttributeMetadataProperties.MaxLength = "MaxLength";
SDK.Metadata.Query.AttributeMetadataProperties.MaxValue = "MaxValue";
SDK.Metadata.Query.AttributeMetadataProperties.MetadataId = "MetadataId";
SDK.Metadata.Query.AttributeMetadataProperties.MinValue = "MinValue";
SDK.Metadata.Query.AttributeMetadataProperties.OptionSet = "OptionSet";
SDK.Metadata.Query.AttributeMetadataProperties.Precision = "Precision";
SDK.Metadata.Query.AttributeMetadataProperties.PrecisionSource = "PrecisionSource";
SDK.Metadata.Query.AttributeMetadataProperties.RequiredLevel = "RequiredLevel";
SDK.Metadata.Query.AttributeMetadataProperties.SchemaName = "SchemaName";
SDK.Metadata.Query.AttributeMetadataProperties.Targets = "Targets";
SDK.Metadata.Query.AttributeMetadataProperties.YomiOf = "YomiOf";
SDK.Metadata.Query.AttributeMetadataProperties.__enum = true;
SDK.Metadata.Query.AttributeMetadataProperties.__flags = true;

//SDK.Metadata.Query.RelationshipMetadataProperties
SDK.Metadata.Query.RelationshipMetadataProperties.prototype = {
 AssociatedMenuConfiguration: "AssociatedMenuConfiguration",
 CascadeConfiguration: "CascadeConfiguration",
 Entity1AssociatedMenuConfiguration: "Entity1AssociatedMenuConfiguration",
 Entity1IntersectAttribute: "Entity1IntersectAttribute",
 Entity1LogicalName: "Entity1LogicalName",
 Entity2AssociatedMenuConfiguration: "Entity2AssociatedMenuConfiguration",
 Entity2IntersectAttribute: "Entity2IntersectAttribute",
 Entity2LogicalName: "Entity2LogicalName",
 IntersectEntityName: "IntersectEntityName",
 IsCustomizable: "IsCustomizable",
 IsCustomRelationship: "IsCustomRelationship",
 IsManaged: "IsManaged",
 IsValidForAdvancedFind: "IsValidForAdvancedFind",
 MetadataId: "MetadataId",
 ReferencedAttribute: "ReferencedAttribute",
 ReferencedEntity: "ReferencedEntity",
 ReferencingAttribute: "ReferencingAttribute",
 ReferencingEntity: "ReferencingEntity",
 RelationshipType: "RelationshipType",
 SchemaName: "SchemaName",
 SecurityTypes: "SecurityTypes"
};
SDK.Metadata.Query.RelationshipMetadataProperties.AssociatedMenuConfiguration = "AssociatedMenuConfiguration";
SDK.Metadata.Query.RelationshipMetadataProperties.CascadeConfiguration = "CascadeConfiguration";
SDK.Metadata.Query.RelationshipMetadataProperties.Entity1AssociatedMenuConfiguration = "Entity1AssociatedMenuConfiguration";
SDK.Metadata.Query.RelationshipMetadataProperties.Entity1IntersectAttribute = "Entity1IntersectAttribute";
SDK.Metadata.Query.RelationshipMetadataProperties.Entity1LogicalName = "Entity1LogicalName";
SDK.Metadata.Query.RelationshipMetadataProperties.Entity2AssociatedMenuConfiguration = "Entity2AssociatedMenuConfiguration";
SDK.Metadata.Query.RelationshipMetadataProperties.Entity2IntersectAttribute = "Entity2IntersectAttribute";
SDK.Metadata.Query.RelationshipMetadataProperties.Entity2LogicalName = "Entity2LogicalName";
SDK.Metadata.Query.RelationshipMetadataProperties.IntersectEntityName = "IntersectEntityName";
SDK.Metadata.Query.RelationshipMetadataProperties.IsCustomizable = "IsCustomizable";
SDK.Metadata.Query.RelationshipMetadataProperties.IsCustomRelationship = "IsCustomRelationship";
SDK.Metadata.Query.RelationshipMetadataProperties.IsManaged = "IsManaged";
SDK.Metadata.Query.RelationshipMetadataProperties.IsValidForAdvancedFind = "IsValidForAdvancedFind";
SDK.Metadata.Query.RelationshipMetadataProperties.MetadataId = "MetadataId";
SDK.Metadata.Query.RelationshipMetadataProperties.ReferencedAttribute = "ReferencedAttribute";
SDK.Metadata.Query.RelationshipMetadataProperties.ReferencedEntity = "ReferencedEntity";
SDK.Metadata.Query.RelationshipMetadataProperties.ReferencingAttribute = "ReferencingAttribute";
SDK.Metadata.Query.RelationshipMetadataProperties.ReferencingEntity = "ReferencingEntity";
SDK.Metadata.Query.RelationshipMetadataProperties.RelationshipType = "RelationshipType";
SDK.Metadata.Query.RelationshipMetadataProperties.SchemaName = "SchemaName";
SDK.Metadata.Query.RelationshipMetadataProperties.SecurityTypes = "SecurityTypes";
SDK.Metadata.Query.RelationshipMetadataProperties.__enum = true;
SDK.Metadata.Query.RelationshipMetadataProperties.__flags = true;


(function () {
 this.OwnershipType = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.OwnershipType enum summary</summary>
  /// <field name="None" type="String" static="true">enum field summary for None</field>
  /// <field name="OrganizationOwned" type="String" static="true">enum field summary for OrganizationOwned</field>
  /// <field name="TeamOwned" type="String" static="true">enum field summary for TeamOwned</field>
  /// <field name="UserOwned" type="String" static="true">enum field summary for UserOwned</field>
  throw new Error("Constructor not implemented this is a static enum.");
 }
 this.AttributeTypeCode = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.AttributeTypeCode enum summary</summary>
  /// <field name="BigInt" type="String" static="true">enum field summary for BigInt</field>
  /// <field name="Boolean" type="String" static="true">enum field summary for Boolean</field>
  /// <field name="CalendarRules" type="String" static="true">enum field summary for CalendarRules</field>
  /// <field name="Customer" type="String" static="true">enum field summary for Customer</field>
  /// <field name="DateTime" type="String" static="true">enum field summary for DateTime</field>
  /// <field name="Decimal" type="String" static="true">enum field summary for Decimal</field>
  /// <field name="Double" type="String" static="true">enum field summary for Double</field>
  /// <field name="EntityName" type="String" static="true">enum field summary for EntityName</field>
  /// <field name="Integer" type="String" static="true">enum field summary for Integer</field>
  /// <field name="Lookup" type="String" static="true">enum field summary for Lookup</field>
  /// <field name="ManagedProperty" type="String" static="true">enum field summary for ManagedProperty</field>
  /// <field name="Memo" type="String" static="true">enum field summary for Memo</field>
  /// <field name="Money" type="String" static="true">enum field summary for Money</field>
  /// <field name="Owner" type="String" static="true">enum field summary for Owner</field>
  /// <field name="PartyList" type="String" static="true">enum field summary for PartyList</field>
  /// <field name="Picklist" type="String" static="true">enum field summary for Picklist</field>
  /// <field name="State" type="String" static="true">enum field summary for State</field>
  /// <field name="Status" type="String" static="true">enum field summary for Status</field>
  /// <field name="String" type="String" static="true">enum field summary for String</field>
  /// <field name="Uniqueidentifier" type="String" static="true">enum field summary for Uniqueidentifier</field>
  /// <field name="Virtual" type="String" static="true">enum field summary for Virtual</field>

  throw new Error("Constructor not implemented this is a static enum.");
 }
  this.RelationshipType = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.RelationshipType enum summary</summary>
  /// <field name="Default" type="String" static="true">enum field summary for Default</field>
  /// <field name="ManyToManyRelationship" type="String" static="true">enum field summary for ManyToManyRelationship</field>  
  /// <field name="OneToManyRelationship" type="String" static="true">enum field summary for OneToManyRelationship</field>  
  throw new Error("Constructor not implemented this is a static enum.");
 }
 this.AttributeRequiredLevel = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel enum summary</summary>
  /// <field name="ApplicationRequired" type="String" static="true">enum field summary for ApplicationRequired</field>
  /// <field name="None" type="String" static="true">enum field summary for None</field>
  /// <field name="Recommended" type="String" static="true">enum field summary for Recommended</field>
  /// <field name="SystemRequired" type="String" static="true">enum field summary for SystemRequired</field>    
  throw new Error("Constructor not implemented this is a static enum.");
 }
 this.DateTimeFormat = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.DateTimeFormat enum summary</summary>
  /// <field name="DateAndTime" type="String" static="true">enum field summary for DateAndTime</field>
  /// <field name="DateOnly" type="String" static="true">enum field summary for DateOnly</field>  
  throw new Error("Constructor not implemented this is a static enum.");
 }
 this.ImeMode = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.ImeMode enum summary</summary>
  /// <field name="Active" type="String" static="true">enum field summary for Active</field>
  /// <field name="Auto" type="String" static="true">enum field summary for Auto</field>
  /// <field name="Disabled" type="String" static="true">enum field summary for Disabled</field>
  /// <field name="Inactive" type="String" static="true">enum field summary for Inactive</field>
  throw new Error("Constructor not implemented this is a static enum.");
 }
 this.IntegerFormat = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.IntegerFormat enum summary</summary>
  /// <field name="Duration" type="String" static="true">enum field summary for Duration</field>
  /// <field name="Language" type="String" static="true">enum field summary for Language</field>
  /// <field name="Locale" type="String" static="true">enum field summary for Locale</field>
  /// <field name="None" type="String" static="true">enum field summary for None</field>
  /// <field name="TimeZone" type="String" static="true">enum field summary for TimeZone</field>
  throw new Error("Constructor not implemented this is a static enum.");
 }
 this.SecurityTypes = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.SecurityTypes enum summary</summary>
  /// <field name="Append" type="String" static="true">enum field summary for Append</field>
  /// <field name="Inheritance" type="String" static="true">enum field summary for Inheritance</field>
  /// <field name="None" type="String" static="true">enum field summary for None</field>
  /// <field name="ParentChild" type="String" static="true">enum field summary for ParentChild</field>
  /// <field name="Pointer" type="String" static="true">enum field summary for Pointer</field>
  throw new Error("Constructor not implemented this is a static enum.");
 }
 this.StringFormat = function () {
  /// <summary>SDK.Metadata.Query.ValueEnums.StringFormat enum summary</summary>
  /// <field name="Email" type="String" static="true">enum field summary for Email</field>
  /// <field name="PhoneticGuide" type="String" static="true">enum field summary for PhoneticGuide</field>
  /// <field name="Text" type="String" static="true">enum field summary for Text</field>
  /// <field name="TextArea" type="String" static="true">enum field summary for TextArea</field>
  /// <field name="TickerSymbol" type="String" static="true">enum field summary for TickerSymbol</field>
  /// <field name="Url" type="String" static="true">enum field summary for Url</field>
  /// <field name="VersionNumber" type="String" static="true">enum field summary for VersionNumber</field>
  throw new Error("Constructor not implemented this is a static enum.");
 }
}).call(SDK.Metadata.Query.ValueEnums);

//	SDK.Metadata.Query.ValueEnums.OwnershipType
SDK.Metadata.Query.ValueEnums.OwnershipType.prototype = {
 None: "None",
 OrganizationOwned: "OrganizationOwned",
 TeamOwned: "TeamOwned",
 UserOwned: "UserOwned"
};
SDK.Metadata.Query.ValueEnums.OwnershipType.None = "None";
SDK.Metadata.Query.ValueEnums.OwnershipType.OrganizationOwned = "OrganizationOwned";
SDK.Metadata.Query.ValueEnums.OwnershipType.TeamOwned = "TeamOwned";
SDK.Metadata.Query.ValueEnums.OwnershipType.UserOwned = "UserOwned";
SDK.Metadata.Query.ValueEnums.OwnershipType.__enum = true;
SDK.Metadata.Query.ValueEnums.OwnershipType.__flags = true;

//	SDK.Metadata.Query.ValueEnums.AttributeTypeCode
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.prototype = {
 BigInt: "BigInt",
 Boolean: "Boolean",
 CalendarRules: "CalendarRules",
 Customer: "Customer",
 DateTime: "DateTime",
 Decimal: "Decimal",
 Double: "Double",
 EntityName: "EntityName",
 Integer: "Integer",
 Lookup: "Lookup",
 ManagedProperty: "ManagedProperty",
 Memo: "Memo",
 Money: "Money",
 Owner: "Owner",
 PartyList: "PartyList",
 Picklist: "Picklist",
 State: "State",
 Status: "Status",
 String: "String",
 Uniqueidentifier: "Uniqueidentifier",
 Virtual: "Virtual"
};
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.BigInt = "BigInt";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Boolean = "Boolean";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.CalendarRules = "CalendarRules";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Customer = "Customer";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.DateTime = "DateTime";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Decimal = "Decimal";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Double = "Double";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.EntityName = "EntityName";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Integer = "Integer";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Lookup = "Lookup";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.ManagedProperty = "ManagedProperty";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Memo = "Memo";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Money = "Money";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Owner = "Owner";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.PartyList = "PartyList";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Picklist = "Picklist";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.State = "State";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Status = "Status";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.String = "String";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Uniqueidentifier = "Uniqueidentifier";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.Virtual = "Virtual";
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.__enum = true;
SDK.Metadata.Query.ValueEnums.AttributeTypeCode.__flags = true;

SDK.Metadata.Query.ValueEnums.RelationshipType.prototype = {
 Default: "Default",
 ManyToManyRelationship: "ManyToManyRelationship",
 OneToManyRelationship: "OneToManyRelationship"
};
SDK.Metadata.Query.ValueEnums.RelationshipType.Default = "Default";
SDK.Metadata.Query.ValueEnums.RelationshipType.ManyToManyRelationship = "ManyToManyRelationship";
SDK.Metadata.Query.ValueEnums.RelationshipType.OneToManyRelationship = "OneToManyRelationship";
SDK.Metadata.Query.ValueEnums.OwnershipType.__enum = true;
SDK.Metadata.Query.ValueEnums.OwnershipType.__flags = true;



//SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel
SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel.prototype = {
 ApplicationRequired: "ApplicationRequired",
 None: "None",
 Recommended: "Recommended",
 SystemRequired: "SystemRequired"
};
SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel.ApplicationRequired = "ApplicationRequired";
SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel.None = "None";
SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel.Recommended = "Recommended";
SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel.SystemRequired = "SystemRequired";
SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel.__enum = true;
SDK.Metadata.Query.ValueEnums.AttributeRequiredLevel.__flags = true;

//SDK.Metadata.Query.ValueEnums.DateTimeFormat
SDK.Metadata.Query.ValueEnums.DateTimeFormat.prototype = {
 DateAndTime: "DateAndTime",
 DateOnly: "DateOnly"
};
SDK.Metadata.Query.ValueEnums.DateTimeFormat.DateAndTime = "DateAndTime";
SDK.Metadata.Query.ValueEnums.DateTimeFormat.DateOnly = "DateOnly";
SDK.Metadata.Query.ValueEnums.DateTimeFormat.__enum = true;
SDK.Metadata.Query.ValueEnums.DateTimeFormat.__flags = true;

//SDK.Metadata.Query.ValueEnums.ImeMode
SDK.Metadata.Query.ValueEnums.ImeMode.prototype = {
 Active: "Active",
 Auto: "Auto",
 Disabled: "Disabled",
 Inactive: "Inactive"
};
SDK.Metadata.Query.ValueEnums.ImeMode.Active = "Active";
SDK.Metadata.Query.ValueEnums.ImeMode.Auto = "Auto";
SDK.Metadata.Query.ValueEnums.ImeMode.Disabled = "Disabled";
SDK.Metadata.Query.ValueEnums.ImeMode.Inactive = "Inactive";
SDK.Metadata.Query.ValueEnums.ImeMode.__enum = true;
SDK.Metadata.Query.ValueEnums.ImeMode.__flags = true;


//SDK.Metadata.Query.ValueEnums.IntegerFormat
SDK.Metadata.Query.ValueEnums.IntegerFormat.prototype = {
 Duration: "Duration",
 Language: "Language",
 Locale: "Locale",
 None: "None",
 TimeZone: "TimeZone"
};
SDK.Metadata.Query.ValueEnums.IntegerFormat.Duration = "Duration";
SDK.Metadata.Query.ValueEnums.IntegerFormat.Language = "Language";
SDK.Metadata.Query.ValueEnums.IntegerFormat.Locale = "Locale";
SDK.Metadata.Query.ValueEnums.IntegerFormat.None = "None";
SDK.Metadata.Query.ValueEnums.IntegerFormat.TimeZone = "TimeZone";
SDK.Metadata.Query.ValueEnums.IntegerFormat.__enum = true;
SDK.Metadata.Query.ValueEnums.IntegerFormat.__flags = true;


//SDK.Metadata.Query.ValueEnums.SecurityTypes
SDK.Metadata.Query.ValueEnums.SecurityTypes.prototype = {
 Append: "Append",
 Inheritance: "Inheritance",
 None: "None",
 ParentChild: "ParentChild",
 Pointer: "Pointer"
};
SDK.Metadata.Query.ValueEnums.SecurityTypes.Append = "Append";
SDK.Metadata.Query.ValueEnums.SecurityTypes.Inheritance = "Inheritance";
SDK.Metadata.Query.ValueEnums.SecurityTypes.None = "None";
SDK.Metadata.Query.ValueEnums.SecurityTypes.ParentChild = "ParentChild";
SDK.Metadata.Query.ValueEnums.SecurityTypes.Pointer = "Pointer";
SDK.Metadata.Query.ValueEnums.SecurityTypes.__enum = true;
SDK.Metadata.Query.ValueEnums.SecurityTypes.__flags = true;

//SDK.Metadata.Query.ValueEnums.StringFormat
SDK.Metadata.Query.ValueEnums.StringFormat.prototype = {
 Email: "Email",
 PhoneticGuide: "PhoneticGuide",
 Text: "Text",
 TextArea: "TextArea",
 TickerSymbol: "TickerSymbol",
 Url: "Url",
 VersionNumber: "VersionNumber"
};
SDK.Metadata.Query.ValueEnums.StringFormat.Email = "Email";
SDK.Metadata.Query.ValueEnums.StringFormat.PhoneticGuide = "PhoneticGuide";
SDK.Metadata.Query.ValueEnums.StringFormat.Text = "Text";
SDK.Metadata.Query.ValueEnums.StringFormat.TextArea = "TextArea";
SDK.Metadata.Query.ValueEnums.StringFormat.TickerSymbol = "TickerSymbol";
SDK.Metadata.Query.ValueEnums.StringFormat.Url = "Url";
SDK.Metadata.Query.ValueEnums.StringFormat.VersionNumber = "VersionNumber";
SDK.Metadata.Query.ValueEnums.StringFormat.__enum = true;
SDK.Metadata.Query.ValueEnums.StringFormat.__flags = true;
//Enumerations part 2  END
