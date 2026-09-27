# Validación del entorno — control del agente `test-reviewer`

**Fecha:** 2026-09-27
**Objetivo usado:** `Coordenadas` (VO `src/Logistica.Domain/Shared/Coordenadas.cs`, factory
`Crear(latitud, longitud)`; catálogo de errores `src/Logistica.Domain/Shared/CoordenadasErrors.cs`)

## Propósito

Comprobar que `test-reviewer` detecta violaciones reales de `.claude/rules/unit-tests.md`
y no se limita a aprobar por defecto. Se ejecutó el flujo real sobre el objetivo `Coordenadas`
y, por separado, se creó una copia saboteada del test generado con tres violaciones
introducidas a propósito. La copia vive en `TestResults/sabotaje/` (fuera de `.gitignore`
tracking y de cualquier `.csproj`; `scripts/test-cobertura.ps1` limpia ese directorio) y
**nunca se integró a la suite real**.

## Informe real — `test-writer` + `test-reviewer` sobre `tests/Logistica.Domain.Tests/Shared/CoordenadasTests.cs`

**Veredicto: CUMPLE**

Puntos comprobados por `test-reviewer`:
- UT-01: solo Domain, sin infraestructura real.
- UT-02: `[Trait("Capa", "Unit")]` presente en la clase.
- UT-03: AAA con comentarios `// Arrange`/`// Act`/`// Assert`, un comportamiento por método.
- UT-04: nombre `Metodo_Escenario_Resultado`. Sin `Ixx` que citar: RN-15 (rango de
  coordenadas) no tiene invariante numerada propia — es una precondición del VO
  (`docs/DESIGN.md` §19) — así que la ausencia del comentario `// Ixx (RN-xx)` es correcta.
- UT-05: caso positivo (límites ±90/±180) y negativo (fuera de rango) por cada eje, con
  `[Theory]`/`[InlineData]`.
- UT-07: `Error.Code` (`COORDENADAS_LATITUD_INVALIDA`, `COORDENADAS_LONGITUD_INVALIDA`) y
  `Error.Type` (`ErrorType.Validation`) coinciden exactamente con el catálogo de
  `docs/DESIGN.md` §13 (líneas 1538-1539).
- UT-11: sin `DateTime.Now`/`UtcNow`, sin `Guid` aleatorio, sin estado compartido.
- UT-12: sin `if`/`for`/`while`; variación de casos vía `[Theory]`.
- UT-14: solo `Assert` de xUnit.
- UT-16: ubicación en espejo (`tests/Logistica.Domain.Tests/Shared/CoordenadasTests.cs` ↔
  `src/Logistica.Domain/Shared/Coordenadas.cs`).
- UT-17 (ejecución dirigida): `dotnet test --filter "FullyQualifiedName~CoordenadasTests"` →
  12/12 en verde.

Sin incoherencias detectadas entre `SISTEMA.md`, `DESIGN.md` y el código.

## Informe de la copia saboteada — `TestResults/sabotaje/CoordenadasTests.Sabotaje.cs`

**Veredicto: NO CUMPLE**

| Archivo:línea | Regla UT-xx | Qué falla | Corrección concreta esperada |
|---|---|---|---|
| `CoordenadasTests.Sabotaje.cs:12` | UT-04 | El método se llama `TestLatitudValida`, que no sigue el patrón `Metodo_Escenario_Resultado` (compárese con `Crear_con_latitud_en_el_limite_90_o_menos_90_es_valida` en el test real). | Renombrar a `Crear_con_latitud_en_el_limite_90_o_menos_90_es_valida`. |
| `CoordenadasTests.Sabotaje.cs:17,24` | UT-11 | Usa `DateTime.Now` como fuente de un valor bajo prueba (línea 17) y lo compara contra otro `DateTime.Now` en el `Assert` (línea 24), justo lo que UT-11 prohíbe. `Coordenadas` no tiene ningún campo temporal, así que la aserción no prueba nada del VO y es no determinista. | Eliminar el uso de `DateTime.Now` y la aserción de marca temporal. |
| `CoordenadasTests.Sabotaje.cs:12-25` | UT-03 (hallazgo adicional, no solicitado) | El método `TestLatitudValida` mezcla dos comportamientos no relacionados: igualdad de `Latitud` y una aserción de reloj. | Dejar un solo comportamiento por método. |
| `CoordenadasTests.Sabotaje.cs:43` | UT-07 | Afirma `ErrorType.Conflict` para `COORDENADAS_LATITUD_INVALIDA`. El catálogo de `docs/DESIGN.md` §13 y `CoordenadasErrors.LatitudInvalida()` declaran `ErrorType.Validation`. | Cambiar a `Assert.Equal(ErrorType.Validation, excepcion.Error.Type);`. |

Resto de reglas (UT-01, UT-02, UT-05, UT-12, UT-14) sí se cumplen en la copia saboteada:
alcance solo Domain, trait presente, casos límite/negativo cubiertos con `Theory`, sin
lógica condicional en el cuerpo del test, solo `Assert` de xUnit.

## Tabla de trazabilidad: violación introducida ↔ fila del informe que la detectó

| # | Violación introducida a propósito | Fila del informe NO CUMPLE que la detectó | Regla citada |
|---|---|---|---|
| 1 | `ErrorType.Conflict` en vez de `ErrorType.Validation` (línea 43, catálogo dice `Validation`) | Fila 4 de la tabla (`CoordenadasTests.Sabotaje.cs:43`) | UT-07 |
| 2 | `DateTime.Now` como fuente de un valor bajo prueba y en la aserción (líneas 17 y 24) | Fila 2 de la tabla (`CoordenadasTests.Sabotaje.cs:17,24`) | UT-11 |
| 3 | Nombre de test `TestLatitudValida`, sin patrón `Metodo_Escenario_Resultado` ni código de invariante | Fila 1 de la tabla (`CoordenadasTests.Sabotaje.cs:12`) | UT-04 |

## Condición de cierre

Las tres violaciones introducidas fueron detectadas por `test-reviewer`, cada una con su
regla `UT-xx` exacta y su corrección concreta. **Condición de cierre cumplida.**
