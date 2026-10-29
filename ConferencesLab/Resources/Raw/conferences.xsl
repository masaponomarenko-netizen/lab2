<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html" indent="yes" encoding="utf-8" />
  <xsl:param name="generated" />

  <xsl:template match="/">
    <html lang="uk">
      <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title>Наукові конференції — звіт</title>
        <style>
          :root {
            --ink: #172033;
            --muted: #5b6780;
            --line: #dfe5ef;
            --accent: #2f5bea;
            --bg: #f3f6fb;
          }
          * { box-sizing: border-box; }
          body {
            margin: 0;
            padding: 32px 20px 56px;
            background: var(--bg);
            color: var(--ink);
            font-family: "Segoe UI", "Helvetica Neue", Arial, sans-serif;
            line-height: 1.45;
          }
          .wrap { max-width: 1080px; margin: 0 auto; }
          header {
            margin-bottom: 24px;
            padding: 28px 32px;
            border-radius: 18px;
            color: #fff;
            background: linear-gradient(135deg, #1c2a52, #2f5bea);
          }
          header h1 { margin: 0 0 6px; font-size: 30px; }
          header p { margin: 0; opacity: .82; }
          .summary {
            display: inline-block;
            margin-top: 14px;
            padding: 6px 12px;
            border-radius: 999px;
            background: rgba(255, 255, 255, .16);
            font-size: 14px;
          }
          .grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(420px, 1fr));
            gap: 18px;
          }
          article {
            padding: 22px 24px;
            border: 1px solid var(--line);
            border-radius: 16px;
            background: #fff;
            box-shadow: 0 10px 28px rgba(23, 32, 51, .07);
          }
          article h2 { margin: 0 0 10px; font-size: 20px; line-height: 1.3; }
          .badges { margin-bottom: 14px; }
          .badge {
            display: inline-block;
            margin: 0 6px 6px 0;
            padding: 3px 10px;
            border-radius: 999px;
            background: #eaf0ff;
            color: #2443a8;
            font-size: 12px;
            font-weight: 600;
          }
          table { width: 100%; border-collapse: collapse; font-size: 14px; }
          th, td { padding: 7px 0; border-top: 1px solid var(--line); vertical-align: top; text-align: left; }
          tr:first-child th, tr:first-child td { border-top: 0; }
          th { width: 38%; color: var(--muted); font-weight: 600; padding-right: 12px; }
          td { color: var(--ink); }
          .child-attr { display: inline-block; margin-left: 8px; color: var(--muted); font-size: 12px; }
          footer { margin-top: 28px; color: var(--muted); font-size: 13px; text-align: center; }
        </style>
      </head>
      <body>
        <div class="wrap">
          <header>
            <h1>Наукові конференції</h1>
            <p>HTML-звіт, сформований з XML-документа за допомогою XSL-трансформації</p>
            <span class="summary">
              Усього конференцій: <xsl:value-of select="count(/conferences/conference)" />
            </span>
          </header>

          <div class="grid">
            <xsl:for-each select="/conferences/conference">
              <article>
                <h2><xsl:value-of select="@title" /></h2>

                <div class="badges">
                  <span class="badge"><xsl:value-of select="@category" /></span>
                  <xsl:if test="@format">
                    <span class="badge"><xsl:value-of select="@format" /></span>
                  </xsl:if>
                  <xsl:if test="@language">
                    <span class="badge"><xsl:value-of select="@language" /></span>
                  </xsl:if>
                </div>

                <table>
                  <xsl:for-each select="@*[name() != 'title' and name() != 'category' and name() != 'format' and name() != 'language']">
                    <tr>
                      <th><xsl:call-template name="label"><xsl:with-param name="name" select="name()" /></xsl:call-template></th>
                      <td>
                        <xsl:choose>
                          <xsl:when test="name() = 'website'">
                            <a href="{.}"><xsl:value-of select="." /></a>
                          </xsl:when>
                          <xsl:otherwise><xsl:value-of select="." /></xsl:otherwise>
                        </xsl:choose>
                      </td>
                    </tr>
                  </xsl:for-each>

                  <xsl:for-each select="*">
                    <tr>
                      <th><xsl:call-template name="label"><xsl:with-param name="name" select="name()" /></xsl:call-template></th>
                      <td>
                        <xsl:value-of select="." />
                        <xsl:for-each select="@*">
                          <span class="child-attr">
                            <xsl:call-template name="label"><xsl:with-param name="name" select="name()" /></xsl:call-template>:
                            <xsl:value-of select="." />
                          </span>
                        </xsl:for-each>
                      </td>
                    </tr>
                  </xsl:for-each>
                </table>
              </article>
            </xsl:for-each>
          </div>

          <footer>
            Звіт сформовано: <xsl:value-of select="$generated" />
          </footer>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="label">
    <xsl:param name="name" />
    <xsl:choose>
      <xsl:when test="$name = 'id'">Ідентифікатор</xsl:when>
      <xsl:when test="$name = 'category'">Тематика</xsl:when>
      <xsl:when test="$name = 'format'">Формат</xsl:when>
      <xsl:when test="$name = 'fee'">Вартість участі</xsl:when>
      <xsl:when test="$name = 'currency'">Валюта</xsl:when>
      <xsl:when test="$name = 'website'">Сайт</xsl:when>
      <xsl:when test="$name = 'language'">Мова</xsl:when>
      <xsl:when test="$name = 'seats'">Кількість місць</xsl:when>
      <xsl:when test="$name = 'dates'">Терміни проведення</xsl:when>
      <xsl:when test="$name = 'start'">початок</xsl:when>
      <xsl:when test="$name = 'end'">завершення</xsl:when>
      <xsl:when test="$name = 'topic'">Тематика</xsl:when>
      <xsl:when test="$name = 'organizer'">Організатор</xsl:when>
      <xsl:when test="$name = 'venue'">Місце проведення</xsl:when>
      <xsl:when test="$name = 'city'">місто</xsl:when>
      <xsl:when test="$name = 'country'">країна</xsl:when>
      <xsl:when test="$name = 'submission'">Подача рукописів</xsl:when>
      <xsl:when test="$name = 'deadline'">дедлайн</xsl:when>
      <xsl:when test="$name = 'contact'">Контакти</xsl:when>
      <xsl:when test="$name = 'email'">email</xsl:when>
      <xsl:otherwise><xsl:value-of select="$name" /></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
