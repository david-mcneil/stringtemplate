StringTemplate.NET - A C# implementation of the StringTemplate Library

1 December, 2005

Kunle Odutola		: kunle UNDERSCORE odutola AT hotmail DOT com
Micheal Jordan


1. INTRODUCTION 

StringTemplate.NET is a new implementation of the StringTemplate MVC 
templating library for the .NET platform. It was developed using the
C# language.

We hope you find 'StringTemplate.NET' delightful and useful even but, 
as per the license under which you may use it, this software is not 
guaranteed to work. See LICENSE.TXT for the full text of the license.

----------------------------------------------------------------------
THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT,
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.
----------------------------------------------------------------------


2. WHAT'S StringTemplate.NET?

StringTemplate is a template engine for generating formatted text output.
Examples of such output includes source code, web pages and emails.

StringTemplate differs from most other template engines by [more] strictly
enforcing Model-View separation.


3. USING StringTemplate.NET

If you have the binary distribution, add references to the following files 
in your project:
  - StringTemplate.dll 
  - antlr.runtime.dll
  
If you have the source distribution, unpack the files into a directory. Open 
the StringTemplate.NET.sln solution file with Visual Studio.NET 2003.


4. CONTRIBUTORS

Kunle Odutola		-- primary developer
Micheal Jordan		-- template loader abstraction, unit tests