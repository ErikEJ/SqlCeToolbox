<?xml version="1.0" encoding="utf-8"?>
<!-- XSL Template for converting the XML documentation to plain text with WikiPlex markup -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="text" indent="no" encoding="utf-8" />

    <xsl:template match="/">
        <xsl:value-of select="concat('! ', /database/@name, ' Database Schema&#13;&#10;&#13;&#10;')" />
        <!-- Process all tables -->
        <xsl:for-each select="/database/object[@type='USER_TABLE' or @type='VIEW']">
            <xsl:sort select="@schema"/>
            <xsl:sort select="@name"/>
            <xsl:call-template name="SingleDbTableOrView" />
        </xsl:for-each>
    </xsl:template>

    <xsl:template name="SingleDbTableOrView">
        <xsl:value-of select="concat('{anchor:', @schema, '.', @name, '}&#13;&#10;!! ')"/>
        <xsl:choose>
            <xsl:when test="@type='USER_TABLE'">Table </xsl:when>
            <xsl:when test="@type='VIEW'">View </xsl:when>
        </xsl:choose>
        <xsl:value-of select="concat(@schema, '.', @name, '&#13;&#10;&#13;&#10;')"/>
        <xsl:if test="@description">
            <xsl:value-of select="concat(@description, '&#13;&#10;&#13;&#10;')" />
        </xsl:if>

        <xsl:text>|| Name || Type || Nullable || Comment ||&#13;&#10;</xsl:text>
        <xsl:for-each select="column">
            <xsl:value-of select="concat('| *', @name, '*')"/>
            <xsl:if test="primaryKey"> ^^PK^^</xsl:if>
            <xsl:value-of select="concat(' | ', @type)"/>
            <xsl:choose>
                <xsl:when test="@length=-1"> (max)</xsl:when>
                <xsl:when test="@type='char' or @type='varchar' or @type='binary' or @type='varbinary'">
                    <xsl:value-of select="concat(' (', @length, ')')"/>
                </xsl:when>
                <xsl:when test="@type='nchar' or @type='nvarchar'">
                    <xsl:value-of select="concat(' (', @length div 2, ')')"/>
                </xsl:when>
                <xsl:when test="@type='real' or @type='money' or @type='float' or @type='decimal' or @type='numeric' or @type='smallmoney'">
                    <xsl:value-of select="concat(' (', @precision, ', ', @scale, ')')"/>
                </xsl:when>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="@nullable='true'"> | NULL | </xsl:when>
                <xsl:otherwise> | NOT NULL | </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="@identity='true'">_IDENTITY_ </xsl:if>
            <xsl:if test="@computed='true'">_COMPUTED_ </xsl:if>
            <xsl:if test="default">
                <xsl:value-of select="concat('_DEFAULT ', default/@value, '_ ')"/>
            </xsl:if>
            <xsl:if test="foreignKey">
                <xsl:variable name="FK" select="foreignKey" />
                <xsl:text>_-&gt; </xsl:text>
                <xsl:value-of select="concat('[#', //object[@id=$FK/@tableId]/@schema, '.', //object[@id=$FK/@tableId]/@name, ']')"/>
                <xsl:value-of select="concat('.', foreignKey/@column, '_ ')"/>
            </xsl:if>
            <xsl:if test="@description">
                <xsl:value-of select="concat(@description, ' ')"/>
            </xsl:if>
            <xsl:text>|&#13;&#10;</xsl:text>
        </xsl:for-each>
        <xsl:text>&#13;&#10;</xsl:text>
    </xsl:template>

</xsl:stylesheet>
