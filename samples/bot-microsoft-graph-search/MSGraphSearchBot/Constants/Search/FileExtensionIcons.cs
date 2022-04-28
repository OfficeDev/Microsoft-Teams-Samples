using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Constants.Search
{
    public static class FileExtensionIcons
    {
        public const string xslx = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAACXBIWXMAAAsTAAALEwEAmpwYAAAEUUlEQVQ4jY2UaUzTdxjH/2/3Zi+XvVoWVA4V5CqXIJdQ7oFCuUo55ZBbt3WUa7S0hZaWclZZ4sKbuZhxuQ03FfAYA5xWJMCYwII4kDGp0alDYvLd8/vXoks07p886b9J+8nz+f6e58dxb3jKz+jeEZ+s9BE2F5UENOV3+2mOmLxUGd+86ff/edK/rH1f9IUsOs74sSKirfSssKX49xBD4XNfbQ68G7Lgqc6AuzINznVJ374VFt8lfRBjPA5hWwlCWooQ1FyAAF0e/LRHsF+TTcBMeKjS4VYvhmt96m1/Q0GDnz6vwVeXq/bR5qi9m7JUnposlUCTqXRRi6VcVOcxhLeXQthajIOGQgQS0P8F0KcxG17UnUApgatCjNQuGQauD+PH2XGqMfzAamYM52Z+5stVLXnMRXaUI4yAoQQMNhxFgD6fB/K6jS91XRSp8FZmIEpfjKiWEiyb1xBlPIbIznKEd5Th6sIkvJuyzVwEfXmpawEeaMol3ZxXdCk/eQq86D2SgBFUc3cXeVhEZxnCOkpxhYBeTVlmLry9DKFtpNtSaAEy3SbSZfk1ElCdTtmJeWDyCRl6xs+jZ+w87q6tov/GCK86OD2Kew/vw1NLQKsuAw7cuozW4a953cGpnzCxOEW6El53X10y9taKsLsqHvZVh2BamIatLBY7K2OxozYO3xPUQ5tp5piuNT/l4Ck8fvoUVf1G/LO5ibRTNXBj46JI4YFJJyosHU5cwMr6GnquXeTfWaesw21gSCvlR8DA5nzM/LGIzWfP8N2tqxCoJBZdAjrVJUFAWYY3FyFMX4SZpXmE6gpwkCqIamh2Ah6aDDMXSkCmax0Xw9BX2NraQtHpRrhbgZSf4+eJcKd3BhQ2F2L2zjxC9EcRrC9AMAEvTI/DvUGywfG6BAyg7pj28sY9PHzyCKPzkzzQpZ7yk1N+BEw0foZe0mS1trGOPprJvutD6LsxjKU/V+CmStvgrLpsXNpHzuCvR2Zkdcspyyd8l850IEx3Dx2IfXU8bCvjsItqamkONrIYfCiLxgcV0QQdgatKvMHxugbS1eeh60ovpL1t8KL5O3m5B+emRnmgIwF31yZA1ClFP9/VMNYf3KfDGOZB/aYRLK+vwoUBLboFOKDLhS/Nn7eGtqMh44WueFvXoSYBLpSl0GDJ8LeVJQTT3AZRhoFkd/HXa3BmQKbrz7aDgPtp/rxo3dgwW8aF6SZv6/JAOpRQAt5evUPAAv4yYcChuV+ow9QNzqKbDz8C+mjpMqDtELDtUFrGxarLgPHGTzFgsigOmC5h4CZ93qTPyUs4S0thLxf9zQXocs3+27rZr9VlQLvqw7CtPrSwT5FsdJQnGp0UiR175Qkde+pE7Q5yURsrm5q4av5O9K4Vv0dDKxQoUqo86iU9rvLUeZq9506v5GdL62ZTEdP/v27s1z07i8PedfjkcLCdNPq4XWVs966Kj0w7pFGn3/a/fwHjz0eXjaFBhgAAAABJRU5ErkJggg==";
        public const string docx = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAACXBIWXMAAAsTAAALEwEAmpwYAAAET0lEQVQ4jY2Ua0ybZRiGv78mxiUbIsjGqQwGDBjngdFoFlHR/VITE5cp4hgwoJwLjI65OUAXT2nLfoAuQ4ej6/nwtaWAgASJGfUEgmyDLQMXXFrKqRyT2+d7WydLXOab3GmatNf7XffztBz3kPPmJ0OPZTf3Z6ZKbSVJ1aZLcZUGR6RYp3rY5x842R9/F/Dy+b7Dz5/pOpvZYDekS61TKXXWrZgqE6LKjdhbqkd4iRZ7ClTGR8IONfbNP3umF2mn7Eg+aUNirRXxEh6xVWZEV5oQWW5AhFiPsGItwoo0k7E1tuboaorE1hRVZWmKrLQ2RpTzjeEV1nNBxSYJ98wHvTjY0I1UqR1JBDxAwDgfcF8FPV2ZASKxDqEEPNzcBeXgJAyOP6F3zEI/MgudL9prswgRm5a5zNM9SD/VjZT6LiTV2ZBQY2HAGAIKuhGlBtLVIaRIg9ea7JAZHWixjuLL/mm09QmZwteDtyGcqAqLi8sgINMlYKIPuL+a9+kaISoVdHUIPqHBq03dkJl+IujPUFjGGLB1GzBSAB5s6PHpeoHxEotP18R0k2stiCbwHgK+p/geTqcTLpeLZWVlBdvP3nLexaVTf2/JhqiPO2woZe0O3LzrxuufDeC5011YXV3Fix/asbtQjdRaAzLrjMigpNca2ftUujCruZ8BI8oImEb9ZTX1Y2trC0cUQzA7ZrCxsYFPTWOovjyCmXtu6k/LgDktA5ifn2dxu91YWFjA4uIiu/QBYHK9HeMzC5DbJnFvwYP5JQ+G/5iDengayqGb1J8WQQVqZDd2Q87/Arn5V8iE8L9RRtFqH8fa2hpEAjBV2s36u0jl/uX2YHNzE+cNY1jxrGHOtUQV/Mj6e5qAr5yze2H8vzAZDafVPgGPx4OwEoOTS6GBJBKw8KtrTPvW3CJb7mXPKrs1Q2olXQ0C81V4RzFwX3N5eZkNRYgAE7RDi3VOTtA9UNeFNAJ71jbQPjBF62LC4PhdTNxxIpj6C6L+Ao6rkCgxIL3OTIMQwiO5hkdSjRkvnLWzy0OL9U4u6aQdCbU2WmYrPiLVNz4fpPEbIekYIfVR0tUy3aeOX8VR+cD9JxOe6p8nE2Dr6+s0PAIKuvEE3E/7F1PF07abaVpGhIkNCKaF9uqq4Z93FVm0Pi3W36HwRW4dp0GOo63nOtsMBhR042q8wGgCCrqiUiNCS/SkqyNdDQKovyePeYEK23bYBEtb7w02zOAio5NL8AFjqy3YV8mTrgnh9PsNIaBXV0O6XqCw4J20RsqhKXoVMo3OH26xKIdvwy9fv8QlVPEur67Fp2vy6eqxm4CBrD8V/I4p4ZfbeSO4UH2BlvxCUKFOEZivUQQUaOX+BTqZkMdztVL2nxie3+4vEqteCi/S1IuKteqQE5rrtHtbgq4A9M9TYdf7SuzIuaL7X//Y/3V2vv3FE/55Fw/tyumo8MvtuLTz3cuOHUe/+fZR3/sb0ZCwWsimOm4AAAAASUVORK5CYII=";
        public const string pptx = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAANPSURBVDhPdVVLSFRhFP7unZlmbpbOpKalZmUiPRY+6ElhtpGSIggqiAgE2wQFtZBCKCipRZtqEdSqbRC0K6JFUVEUPSkwTScVU1JMZ5y56n11zvnvnRTskzPnP/ec853zP9U8AhaAZ1uYHh5CdqAHmf5eZAf7EFlWiKq2dj9iYQihk0nB/DUAc6AX00P9yPzqh/1nDJaZhee48FyHxEXBxnpUn77kpy4MrfvaWc9OT8K1LElS4sBlIm++HSteiZLtjfDoTxDMjSepAXokCt1JTVAXjnKygyS3CvNsD240htn8QsRKKxBlWcG6XOmScvx+dB+6ilUJLLnqgRZSpcPxBGIbGqDX1NEHDelPb2CsqUF8cyPiW3YjZCyGrhICEYocAYsUCPyWjaF7N9B3uxOp38MYf/UEP66chp1J+3keEc5lCYWwfO9hFDcfglG5jvxMpHyssj+7kfr8Gu50lrgdmJYDizZv8v0LDpL4cK46QdNDKGraD5N2unDPAfTfvYr0148+sQc7PcFrT2tuI167DfmbGhA1DCwpKpF8hpoyQSk1Hr5/B5MfXiLesEt9CwpqHp8zZHu70HW+Fd0dbRh5/ACzdAIC0JQ53k/yE2MVa2GsWgdrfEz5GKQjiWIpqcmvh1A4gsKdzXDolMzMzEiMrsJVQJBcevAErNERjD59KLaAfAYVKm05iryq9YjX7UBV+3UYZZXUuQaXzqvt5tZQJQT4eesiMsnvEqT8arc1jTZt3xGUEKmu60LEYM3CFyEsbH6SCEOISAe2+NQu91w+Reuopi10FBvOW4rqjpsSqjokp6TSFZt49xx2Nk2fmUSJeEkbldWoPNme64gRjHP2t3PH6Lqqyx/IP9u/0/7YWF2NxJYmSubyREJKdn5RFEWNLehqP86bwtWFnMAD35bu/G/cJdsEboSoZLqqKWVJhxSTO4cBgUqUgRoHtkBDOL+AJI5wAUl+AhHSITrck2+fwTVNaF8utHpeZorap6nOmR7vMGt5D8Xn0PNVhmX1WyUmRLvMtdT6qXKhvCXqgbXoSk0NJpFN/qCXuQcmvc6zdPmZEHS2gjVN1G5HzblOlf0f/PdfgG1mkEp2YyrZg5nBXirYh9jycqw/c9GPWAjAXxPdI2ghwFpkAAAAAElFTkSuQmCC";
        public const string pdf = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAACxMAAAsTAQCanBgAAAPtSURBVDhPXVRLiFtVGP7uI4/JY+Ikk07Sap1IGR18LBQVRkGh0Fk5gkiLUKgufEBpRRQX4saCbuxqtHQp2q6GtkxxZLQrcRSciki1LTIFa8tkppk0kzaPm5t7zr3X79w8mviRP+cm95zvfP9TQxczMzMPvf/Uo5+GTeM+QFd/+fxoPhfP8+BINxTOZGPx6Se+rdfrX7zy1tuN4OD/0Cecm5t75ljCW45qSHuGgVgoBF3ToekaNO5yPR8tw0R775zdnMifqLfsT2ZnZ+vd430EUnpoWS3cul2BZdlw2gK+6wbmudTpU6ndglHbjj4wOXkkHo/PLyws9AX1MEQYpYJMIslVg+67cKWE8IBafJSk6rcHV0hks9lwoVA4mM/nP19eXh7vHg8wRAjpIOS5CEkq40FPCOjJUaSfe5FELqQU9F3AsSwk4jFzZy531Krcfm9+ft7sMqD/oCAdB5JO6FTq6TpXGpXpiVG4jg1BheWl87i6eAaCIYhkc6Hc0zPvrBuR73l8RXEMKZTtNlyaInYFzaGaeg1mPA5BhYo8yXBMRMK4f2QEmUYN9sWVcLFYTHcphgkVmSCRbLcCcunwAqoNKYU7dwfhGGXWM6aJbNjEjoiJMemI1dXVcpfiHqHJTVLYkDZNkXF12jYwuQfV785i7IW9cMbGYdudi3gzIr6HCIs1kUi4XZp7hBqLzXW5WZKQ8VJmw0By+jFsXbmMaDiCwptHkT3yIVIHDsGcfjxIms9EGYbBJuhgyGXhqzJpo+3aaJghpF/eD61cQmoij/UTx/Hvx+9CLp1B7G4VEZZUkER6M4hhQtYcEwnsKmDXocNUFcbNUydR/vkC7OoWZKuJO5d+R3HhG1R/+6WTRJIOYohQshQcWvqRJxFjFouLp9Bu1SBUKKhaqBgzfoJJEyqBjLFkEulyl2GAULWWZL+6XDd+WsLfJz9DvVQMLlA1J5hh4Tk0kkheEJDTSKqzZnvoP0nVZsFBDoFmA6VqFdusPcv1sMV6rDoSNt9tMxFlklgqeYqY3TWIfqeEOF1U/HQeCiczePaDY9BTKZTOn8buV1+HtbmJjQuL2PPSftibN3Hnh3Nw1q8HXg1iOIacfco81uTIeAbRkAEjGkX92jXU/rmK3PP7EM1wFrBU2o1G4I3ySnnXwxChip/khWrKVP78AyuHX0NtYx3hXB7JBx9G5dcfYd24gfVzp2FVSkESVWWo+PfQd5m3+Byivs6XVq2Cy19/GQzV8pVLqB3/KOgMt1nH1l8X4Vl1aNyn1Eg+KHRYBib21NRUfl/z1hvwvZT6MzB+dTZwPnaWYO294zznRNfvno3s+GptbW0TAP4DeRg6q1Veu70AAAAASUVORK5CYII=";
        public const string image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAACXBIWXMAAAsTAAALEwEAmpwYAAAC2klEQVQ4ja2U609SYRjA+Z/6D5yAXAsO56RcRAUkkUtOzWXTqSvn8kOampamJqJrXjAlE3NSkylawkrQMEFLUJGLolDmoqcDDjcGWqzO9tue9/nwO+9z2UsgUlgZJBq78H9ApCMkAomB7uQUyDz5RTdd/wJXVOwhM1A/gcLEPG/MNtgKniQwMD4NQ1OGpPxFGK3rkMXADhOExhU7qLVTsbhPOwkDuplY3Duig4VVR/rCpfUvIJQqwbTmhOYuNbR0a2Duox0EEgUsO1zpC6NY3T5AuXlwt0YBtdXFkJ0rgk97B+mXPDjxGm7VNOB9mwW5UgoRV3sMiVQMozNzUFZ179KeJgnfb2zDpPEd6E0WwHgC8K42g2flAbAxHhgsNtC9XQSz051+ya/ml6G+qRWymGiM+61PQL9gThK09T33ShTl/rXdwMXCF4Z54PDxnu0fgsnmBJN9C6zbXmDl5MV+FJd1aIYC5KvXw6xcmU9YqAzYPQephRu+Y7DhQ4nGjY+6oamr/3xQDn/4bIWGJ/xkJvZd1ToWqdTMA6tAtS+UqgKzZmvqkuMYLKv4Xn5OyPWMvAxkRWUt2sidwUWIEpPmKb3cAlmQzMCCKYUzSx9+MhBuCOWLw5bNnVjuGS4jMbET5cPRc1mc22ojMHMkQSIVOU0S9gyNh/Cr/6jrffqtqKp6l8MThx5rRo5isubhJFkcVcsYEGlo+FzoCITx6XbsU1nZ4Tb9eGRsywrazRWQ19a5iTQkcqO+59dFsigl7Tog0tGzkqP7J5KX+hChOKA2GyEqi3MmrXVT2YJQWef0n4V4eX4mhx+WVFT6htctCbJEaZ2LwuIdl3XqLxfij+JJSUNDKJUoSVpT+5WC8I/KU9xU1tgfwd/WDQKZztnjFSn8+aXle38DA+N7SXTOKUsg32QJFY4o13KLnSQ6GiJS2SJCJhVhZ9KQivRgqzMoSGX8jK9LCYmGXiHg328yTk6jlTA4BQAAAABJRU5ErkJggg==";
        public const string music = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAz0lEQVQ4je3UqwoCQRiG4VERBLHYDcpWi+I1GMVgsBrE6g2IRcSgGGxWg10EMQiLRpvXYDV5SB5eWIMss+w/YzG48LRv32HDjlJ2Twx5NDBA0jTgoI4Rtjjj+aEgibSxxsn3sk5REnwIQkbBsMgRrm3whh2GqCHz3kRNg2OUEA/ZiYOS4T/4Y8Hst8E0+ljhgIs06AQMZ0r466XQwQJ7XAOGm4Bgzh+cCk+u4u7bLHWf6Crhycq7+7qYoIWELtjUxOa6oclTVt6N3EMFEZvICwHib3W2LfNuAAAAAElFTkSuQmCC";
        public const string video = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAACXBIWXMAAAsTAAALEwEAmpwYAAAApUlEQVQ4jdWUQQ5GMBBG5wIILqVsJMJV/m2PaY17MJKvyWja/IxuTPIWyjzVr0r0hWqZldkfcvaYkFAjcyyey9ILmUPK9lRCK69jnzIwGeiZOfKs9V8QklWBtS5x7++M/YEBgo7Z6JrmqBFmaJbpuzTz1MJCI+zRbCA9ZQ3GJo1wRgB+1XTvJ4humxFrlmNmMZm9I3y6sX+phVKa/nAwSqlM/0N1AHGS/dFxbLpqAAAAAElFTkSuQmCC";
        public const string file = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAnUlEQVQ4jWNgAAIBMUU9fkmFNKKwuEIZn6R8JAM+ICChsEdAXOEdkL5LBH4PxP8FxWUb8Bm4n19cHrcCJABSJyAu/wdiqHwTdQyEqD8AMhQYBC1UMRDKxm4oGV6+CY8kCfmXIEOFJORqyDNQQi4YPaL4JRQ+AcPzAVkG4nQ1NBhGDRw1cNRAUgwkpYDFxBC9exAGklIF4MAgM0BmAQBWHoUNVgW0TgAAAABJRU5ErkJggg==";
        public const string code = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAACXBIWXMAAAsTAAALEwEAmpwYAAAA60lEQVQ4jWNgGE7ADIizgZidGoaxAPE7IP4PxDnUMFAXahgIx5OicTIQfwRifzTxRCQDdaBiIDUgV0/EZVgpkqZuNLkpUPGvDBDvg0AHkvpSdMN8gfgvVPI+EIuhyR+Hyh1FEhODqv0P1esLk9AC4g9Qic9ArIdmGDPUZSD5SWhyWPVex2YLEtBjwB8hyL67xgAl8BmILUJwGXgVp7ORALYIgQGcevFFCrYIYWDAEykwgJxs2qFi+CKkkwFPsoEBUCJ9i2Qbvgjxg6rFmbCxAeQI0SZFIy6QAzUMlMXQI4QsACqqshggRdcwAQAzK14NAoTdyQAAAABJRU5ErkJggg==";
        public static string getIconByFileType(string fileExtension)
        {
            switch (fileExtension)
            {
                case "xsl":
                case "xlsx":
                    return xslx;
                case "docx":
                case "doc":
                    return docx;
                case "ppt":
                case "pptx":
                    return pptx;
                case "pdf":
                    return pdf;
                case "jpeg":
                case "png":
                case "gif":
                case "bmp":
                    return image;
                case "mpg":
                case "mpeg":
                case "mp4":
                case "wmv":
                case "avi":
                    return video;
                case "mp3":
                case "audio":
                case "wav":
                case "voc":
                    return music;
                case "js":
                case "css":
                case "ts":
                case "json":
                case "xml":
                    return code;
                default:
                    return file;
            }
        }
    }
}
