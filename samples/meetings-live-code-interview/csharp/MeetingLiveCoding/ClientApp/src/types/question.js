// Sample question details. 
export const IQuestionDetails = {
    questions: [
        {
            questionId : 1,
            question:"Write a program to accept N numbers and arrange them in an ascending order",
            expectedOuput:"For array with 7 elements containing [6,3,7,4,2,0,8] the output will be [0,2,3,4,6,7,8]",
            language:"csharp",
            defaultValue:"using System;\nnamespace SortAnArray\n{\n public class SortAnArray\n  {\n    static void Main(string[] args) { \n     \/\/ Write your code here \n   }\n  }\n}"
        },
        {
            questionId : 2,
            question:"Get unique value from array eg:[1,4,5,4,2,1,9,5,7,6,3]",
            expectedOuput:"[1,4,5,2,9,6,3]",
            language:"javascript",
            defaultValue:"const arr = [1,4,5,4,2,1,9,5,7,6,3]; \n\n function getUniqueElement(arr){ \n   \/\/write your code here\n }"
        },
        {
            questionId : 3,
            question:"Get count of character from string",
            expectedOuput:"if string is 'javascript' then output will be {j:1,a:2,v:1,s:1,c:1,r:1,i:1,p:1,t:1}",
            language:"javascript",
            defaultValue:"const str = 'javascript' \n\n function getCount(str){\n   \/\/ write your code here\n }"
        }
    ]
};