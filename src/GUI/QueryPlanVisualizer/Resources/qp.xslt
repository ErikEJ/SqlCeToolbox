<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:s="http://schemas.microsoft.com/sqlserver/2004/07/showplan"
  exclude-result-prefixes="msxsl s xsl">
  <xsl:output method="html" indent="no" omit-xml-declaration="yes" />

  <!-- Disable built-in recursive processing templates -->
  <xsl:template match="*|/|text()|@*" mode="NodeLabel" />
  <xsl:template match="*|/|text()|@*" mode="ToolTipDescription" />
  <xsl:template match="*|/|text()|@*" mode="ToolTipDetails" />

  <!-- Default template -->
  <xsl:template match="/">
    <xsl:apply-templates select="s:ShowPlanXML" />
  </xsl:template>

  <!-- Outermost div that contains all statement plans. -->
  <xsl:template match="s:ShowPlanXML">
    <div class="qp-root">
      <xsl:apply-templates select="s:BatchSequence/s:Batch/s:Statements/s:StmtSimple" />  
    </div>
  </xsl:template>
  
  <!-- Matches a statement. -->
  <xsl:template match="s:StmtSimple">
    <div class="qp-tr">
      <div>
        <div class="qp-node">
          <xsl:element name="div">
            <xsl:attribute name="class">qp-icon-Statement</xsl:attribute>
          </xsl:element>
          <div><xsl:value-of select="@StatementType" /></div>
          <xsl:apply-templates select="." mode="NodeLabel" />
          <xsl:call-template name="ToolTip" />
        </div>
      </div>
      <div><xsl:apply-templates select="*/s:RelOp" /></div>
    </div>
  </xsl:template>
  
  <!-- Matches a branch in the query plan. -->
  <xsl:template match="s:RelOp">
    <div class="qp-tr">
      <div>
        <div class="qp-node">
          <xsl:element name="div">
            <xsl:attribute name="class">qp-icon-<xsl:value-of select="translate(@PhysicalOp, ' ', '')" /></xsl:attribute>
          </xsl:element>
          <div><xsl:value-of select="@PhysicalOp" /></div>
          <xsl:apply-templates select="." mode="NodeLabel" />
          <xsl:apply-templates select="." mode="NodeCostLabel" />
          <xsl:call-template name="ToolTip" />
        </div>
      </div>
      <div><xsl:apply-templates select="*/s:RelOp" /></div>
    </div>
  </xsl:template>

  <!-- Writes the tool tip -->
  <xsl:template name="ToolTip">
    <div class="qp-tt">
      <div class="qp-tt-header"><xsl:value-of select="@PhysicalOp | @StatementType" /></div>
      <div><xsl:apply-templates select="." mode="ToolTipDescription" /></div>
      <xsl:call-template name="ToolTipGrid" />
      <xsl:apply-templates select="* | @* | */* | */@*" mode="ToolTipDetails" />
    </div>
  </xsl:template>

  <!-- Writes the grid of node properties to the tool tip -->
  <xsl:template name="ToolTipGrid">
    <table>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Condition" select="s:QueryPlan/@CachedPlanSize" />
        <xsl:with-param name="Label">Cached plan size</xsl:with-param>
        <xsl:with-param name="Value" select="concat(s:QueryPlan/@CachedPlanSize, ' B')" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Physical Operation</xsl:with-param>
        <xsl:with-param name="Value" select="@PhysicalOp" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Logical Operation</xsl:with-param>
        <xsl:with-param name="Value" select="@LogicalOp" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Actual Number of Rows</xsl:with-param>
        <xsl:with-param name="Value" select="s:RunTimeInformation/s:RunTimeCountersPerThread/@ActualRows" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Condition" select="@EstimateIO" />
        <xsl:with-param name="Label">Estimated I/O Cost</xsl:with-param>
        <xsl:with-param name="Value">
          <xsl:call-template name="round">
            <xsl:with-param name="value" select="@EstimateIO" />
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Condition" select="@EstimateCPU" />
        <xsl:with-param name="Label">Estimated CPU Cost</xsl:with-param>
        <xsl:with-param name="Value">
          <xsl:call-template name="round">
            <xsl:with-param name="value" select="@EstimateCPU" />
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
      <!-- TODO: Estimated Number of Executions -->
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Number of Executions</xsl:with-param>
        <xsl:with-param name="Value" select="s:RunTimeInformation/s:RunTimeCountersPerThread/@ActualExecutions" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Degree of Parallelism</xsl:with-param>
        <xsl:with-param name="Value" select="s:QueryPlan/@DegreeOfParallelism" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Memory Grant</xsl:with-param>
        <xsl:with-param name="Value" select="s:QueryPlan/@MemoryGrant" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Condition" select="@EstimateIO | @EstimateCPU" />
        <xsl:with-param name="Label">Estimated Operator Cost</xsl:with-param>
        <xsl:with-param name="Value">
          <xsl:variable name="EstimatedOperatorCost">
            <xsl:call-template name="EstimatedOperatorCost" />
          </xsl:variable>
          <xsl:variable name="TotalCost">
            <xsl:value-of select="ancestor::s:StmtSimple/@StatementSubTreeCost" />
          </xsl:variable>
          
          <xsl:call-template name="round">
            <xsl:with-param name="value" select="$EstimatedOperatorCost" />
          </xsl:call-template>
          (<xsl:value-of select="format-number(number($EstimatedOperatorCost) div number($TotalCost), '0%')" />)
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Condition" select="@StatementSubTreeCost | @EstimatedTotalSubtreeCost" />
        <xsl:with-param name="Label">Estimated Subtree Cost</xsl:with-param>
        <xsl:with-param name="Value">
          <xsl:call-template name="round">
            <xsl:with-param name="value" select="@StatementSubTreeCost | @EstimatedTotalSubtreeCost" />
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Estimated Number of Rows</xsl:with-param>
        <xsl:with-param name="Value" select="@StatementEstRows | @EstimateRows" />
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Condition" select="@AvgRowSize" />
        <xsl:with-param name="Label">Estimated Row Size</xsl:with-param>
        <xsl:with-param name="Value" select="concat(@AvgRowSize, ' B')" />
      </xsl:call-template>
      <!-- TODO: Actual Rebinds
           TODO: Actual Rewinds -->
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Condition" select="s:IndexScan/@Ordered" />
        <xsl:with-param name="Label">Ordered</xsl:with-param>
        <xsl:with-param name="Value">
          <xsl:choose>
            <xsl:when test="s:IndexScan/@Ordered = 1">True</xsl:when>
            <xsl:otherwise>False</xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="ToolTipRow">
        <xsl:with-param name="Label">Node ID</xsl:with-param>
        <xsl:with-param name="Value" select="@NodeId" />
      </xsl:call-template>
    </table>
  </xsl:template>

  <!-- Calculates the estimated operator cost. -->
  <xsl:template name="EstimatedOperatorCost">
    <xsl:variable name="EstimateIO">
      <xsl:call-template name="convertSciToNumString">
        <xsl:with-param name="inputVal" select="@EstimateIO" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="EstimateCPU">
      <xsl:call-template name="convertSciToNumString">
        <xsl:with-param name="inputVal" select="@EstimateCPU" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="number($EstimateIO) + number($EstimateCPU)" />
  </xsl:template>

  <!-- Renders a row in the tool tip details table. -->
  <xsl:template name="ToolTipRow">
    <xsl:param name="Label" />
    <xsl:param name="Value" />
    <xsl:param name="Condition" select="$Value" />
    <xsl:if test="$Condition">
      <tr>
        <th><xsl:value-of select="$Label" /></th>
        <td><xsl:value-of select="$Value" /></td>
      </tr>      
    </xsl:if>
  </xsl:template>

  <!-- Prints the name of an object. -->
  <xsl:template match="s:Object | s:ColumnReference" mode="ObjectName">
    <xsl:for-each select="@Database | @Schema | @Table | @Index | @Column | @Alias">
      <xsl:value-of select="." />
      <xsl:if test="position() != last()">.</xsl:if>
    </xsl:for-each>
  </xsl:template>
  
  <!-- 
  ================================
  Tool tip detail sections
  ================================
  The following section contains templates used for writing the detail sections at the bottom of the tool tip,
  for example listing outputs, or information about the object to which an operator applies.
  -->

  <xsl:template match="*/s:Object" mode="ToolTipDetails">
    <!-- TODO: Make sure this works all the time -->
    <div class="qp-bold">Object</div>
    <div><xsl:apply-templates select="." mode="ObjectName" /></div>
  </xsl:template>

  <xsl:template match="s:SetPredicate[s:ScalarOperator/@ScalarString]" mode="ToolTipDetails">
    <div class="qp-bold">Predicate</div>
    <div><xsl:value-of select="s:ScalarOperator/@ScalarString" /></div>
  </xsl:template>

  <xsl:template match="s:OutputList[count(s:ColumnReference) > 0]" mode="ToolTipDetails">
    <div class="qp-bold">Output List</div>
    <xsl:for-each select="s:ColumnReference">
      <div><xsl:apply-templates select="." mode="ObjectName" /></div>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="s:NestedLoops/s:OuterReferences[count(s:ColumnReference) > 0]" mode="ToolTipDetails">
    <div class="qp-bold">Outer References</div>
    <xsl:for-each select="s:ColumnReference">
      <div><xsl:apply-templates select="." mode="ObjectName" /></div>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="@StatementText" mode="ToolTipDetails">
    <div class="qp-bold">Statement</div>
    <div><xsl:value-of select="." /></div>
  </xsl:template>

  <xsl:template match="s:Sort/s:OrderBy[count(s:OrderByColumn/s:ColumnReference) > 0]" mode="ToolTipDetails">
    <div class="qp-bold">Order By</div>
    <xsl:for-each select="s:OrderByColumn">
      <div>
        <xsl:apply-templates select="s:ColumnReference" mode="ObjectName" />
        <xsl:choose>
          <xsl:when test="@Ascending = 1"> Ascending</xsl:when>
          <xsl:otherwise> Descending</xsl:otherwise>
        </xsl:choose>
      </div>
    </xsl:for-each>
  </xsl:template>

  <!-- TODO: Seek Predicates -->

  <!--
  ================================
  Operator specific node labels
  ================================
  The following section contains templates used for writing operator-type specific node labels.
  -->

  <xsl:template match="s:RelOp" mode="NodeCostLabel">
    <xsl:variable name="EstimatedOperatorCost">
      <xsl:call-template name="EstimatedOperatorCost" />
    </xsl:variable>
    <xsl:variable name="TotalCost">
      <xsl:value-of select="ancestor::s:StmtSimple/@StatementSubTreeCost" />
    </xsl:variable>
    <div>Cost: <xsl:value-of select="format-number(number($EstimatedOperatorCost) div number($TotalCost), '0%')" /></div>
  </xsl:template>
  
  <!-- Node label for "Nested Loops" operation -->
  <xsl:template match="*[s:NestedLoops]" mode="NodeLabel">
    <div>(<xsl:value-of select="@LogicalOp" />)</div>
  </xsl:template>

  <!-- Node label for "Index Scan" operation -->
  <xsl:template match="*[s:IndexScan]" mode="NodeLabel">
    <xsl:variable name="IndexName" select="concat(s:IndexScan/s:Object/@Table, '.', s:IndexScan/s:Object/@Index)" />
    <div>
      <xsl:value-of select="substring($IndexName, 0, 36)" />
      <xsl:if test="string-length($IndexName) >= 36">…</xsl:if>
    </div>
  </xsl:template>

  <xsl:template match="*[s:TableScan]" mode="NodeLabel">
    <xsl:variable name="IndexName" select="concat(s:TableScan/s:Object/@Schema, '.', s:TableScan/s:Object/@Table)" />
    <div>
      <xsl:value-of select="substring($IndexName, 0, 36)" />
      <xsl:if test="string-length($IndexName) >= 36">…</xsl:if>
    </div>
  </xsl:template>

  <!-- 
  ================================
  Tool tip descriptions
  ================================
  The following section contains templates used for writing the description shown in the tool tip.
  -->

  <xsl:template match="*[@PhysicalOp = 'Table Insert']" mode="ToolTipDescription">Insert input rows into the table specified in Argument field.</xsl:template>
  <xsl:template match="*[@PhysicalOp = 'Compute Scalar']" mode="ToolTipDescription">Compute new values from existing values in a row.</xsl:template>
  <xsl:template match="*[@PhysicalOp = 'Sort']" mode="ToolTipDescription">Sort the input.</xsl:template>
  <xsl:template match="*[@PhysicalOp = 'Clustered Index Scan']" mode="ToolTipDescription">Scanning a clustered index, entirely or only a range.</xsl:template>
  <xsl:template match="*[s:TableScan]" mode="ToolTipDescription">Scan rows from a table.</xsl:template>
  <xsl:template match="*[s:NestedLoops]" mode="ToolTipDescription">For each row in the top (outer) input, scan the bottom (inner) input, and output matching rows.</xsl:template>
  <xsl:template match="*[s:Top]" mode="ToolTipDescription">Select the first few rows based on a sort order.</xsl:template>

  <!-- 
  ================================
  Number handling
  ================================
  The following section contains templates used for handling numbers (scientific notation, rounding etc...)
  -->

  <!-- Outputs a number rounded to 7 decimal places - to be used for displaying all numbers.
  This template accepts numbers in scientific notation. -->
  <xsl:template name="round">
    <xsl:param name="value" select="0" />
    <xsl:variable name="number">
      <xsl:call-template name="convertSciToNumString">
        <xsl:with-param name="inputVal" select="$value" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="round(number($number) * 10000000) div 10000000" />
  </xsl:template>
  
  <!-- Template for handling of scientific numbers
  See: http://www.orm-designer.com/article/xslt-convert-scientific-notation-to-decimal-number -->
  <xsl:variable name="max-exp">
    <xsl:value-of select="'0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000'" />
  </xsl:variable>

  <xsl:template name="convertSciToNumString">
    <xsl:param name="inputVal" select="0" />

    <xsl:variable name="numInput">
      <xsl:value-of select="translate(string($inputVal),'e','E')" />
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="number($numInput) = $numInput">
        <xsl:value-of select="$numInput" />
      </xsl:when> 
      <xsl:otherwise>
        <!-- ==== Mantisa ==== -->
        <xsl:variable name="numMantisa">
          <xsl:value-of select="number(substring-before($numInput,'E'))" />
        </xsl:variable>

        <!-- ==== Exponent ==== -->
        <xsl:variable name="numExponent">
          <xsl:choose>
            <xsl:when test="contains($numInput,'E+')">
              <xsl:value-of select="substring-after($numInput,'E+')" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="substring-after($numInput,'E')" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <!-- ==== Coefficient ==== -->
        <xsl:variable name="numCoefficient">
          <xsl:choose>
            <xsl:when test="$numExponent > 0">
              <xsl:text>1</xsl:text>
              <xsl:value-of select="substring($max-exp, 1, number($numExponent))" />
            </xsl:when>
            <xsl:when test="$numExponent &lt; 0">
              <xsl:text>0.</xsl:text>
              <xsl:value-of select="substring($max-exp, 1, -number($numExponent)-1)" />
              <xsl:text>1</xsl:text>
            </xsl:when>
            <xsl:otherwise>1</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="number($numCoefficient) * number($numMantisa)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
